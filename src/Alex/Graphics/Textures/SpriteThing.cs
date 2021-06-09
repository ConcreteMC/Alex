using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;

namespace Alex.Graphics.Textures
{
    /// <summary>
    /// Objects that performs the packing task. Takes a list of textures as input and generates a set of atlas textures/definition pairs
    /// </summary>
    public class Packer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Packer));
        
        /// <summary>
        /// Number of pixels that separate textures in the atlas
        /// </summary>
        public int Padding;
        
        /// <summary>
        /// Size of the atlas in pixels. Represents one axis, as atlases are square
        /// </summary>
        public int AtlasSize;

        /// <summary>
        /// Which heuristic to use when doing the fit
        /// </summary>
        public BestFitHeuristic FitHeuristic;

        public Packer()
        {
        }

        /// <summary>
        /// Heuristic guesses what might be a good output width for a list of sprites.
        /// </summary>
        public static int GuessOutputWidth(List<TextureInfo> sprites)
        {
            // Gather the widths of all our sprites into a temporary list.
            List<int> widths = new List<int>();
 
            foreach (var sprite in sprites)
            {
                widths.Add(sprite.Width);
            }
 
            // Sort the widths into ascending order.
            widths.Sort();
 
            // Extract the maximum and median widths.
            int maxWidth = widths[widths.Count - 1];
            int medianWidth = widths[widths.Count / 2];
 
            // Heuristic assumes an NxN grid of median sized sprites.
            int width = medianWidth * (int)Math.Round(Math.Sqrt(sprites.Count));
 
            // Make sure we never choose anything smaller than our largest sprite.
            return Math.Max(width, maxWidth);
        }
        
        public IEnumerable<Atlas> Process(List<TextureInfo> sourceTextures, int atlasSize, int padding)
        {
            Padding = padding;
            
            AtlasSize = atlasSize;
            
            //2: generate as many atlasses as needed (with the latest one as small as possible)
            while (sourceTextures.Count > 0)
            {
                Atlas atlas = new Atlas();
                atlas.Width = atlasSize;
                atlas.Height = atlasSize;

                List<TextureInfo> leftovers = LayoutAtlas(sourceTextures, atlas);

                if (leftovers.Count == 0)
                {
                    // we reached the last atlas. Check if this last atlas could have been twice smaller
                    while (leftovers.Count == 0)
                    {
                        atlas.Width /= 2;
                        atlas.Height /= 2;
                        leftovers = LayoutAtlas(sourceTextures, atlas);
                    }
                    // we need to go 1 step larger as we found the first size that is to small
                    atlas.Width *= 2;
                    atlas.Height *= 2;
                    leftovers = LayoutAtlas(sourceTextures, atlas);
                }

                yield return atlas;
                //Atlasses.Add(atlas);

                sourceTextures = leftovers;
            }
        }

        private void HorizontalSplit(Node toSplit, int width, int height, List<Node> list)
        {
            Node n1 = new Node();
            n1.Bounds.X = toSplit.Bounds.X + width + Padding;
            n1.Bounds.Y = toSplit.Bounds.Y;
            n1.Bounds.Width = toSplit.Bounds.Width - width - Padding;
            n1.Bounds.Height = height;
            n1.SplitType = SplitType.Vertical;

            Node n2 = new Node();
            n2.Bounds.X = toSplit.Bounds.X;
            n2.Bounds.Y = toSplit.Bounds.Y + height + Padding;
            n2.Bounds.Width = toSplit.Bounds.Width;
            n2.Bounds.Height = toSplit.Bounds.Height - height - Padding;
            n2.SplitType = SplitType.Horizontal;

            if (n1.Bounds.Width > 0 && n1.Bounds.Height > 0)
                list.Add(n1);
            if (n2.Bounds.Width > 0 && n2.Bounds.Height > 0)
                list.Add(n2);
        }

        private void VerticalSplit(Node toSplit, int width, int height, List<Node> list)
        {
            Node n1 = new Node();
            n1.Bounds.X = toSplit.Bounds.X + width + Padding;
            n1.Bounds.Y = toSplit.Bounds.Y;
            n1.Bounds.Width = toSplit.Bounds.Width - width - Padding;
            n1.Bounds.Height = toSplit.Bounds.Height;
            n1.SplitType = SplitType.Vertical;

            Node n2 = new Node();
            n2.Bounds.X = toSplit.Bounds.X;
            n2.Bounds.Y = toSplit.Bounds.Y + height + Padding;
            n2.Bounds.Width = width;
            n2.Bounds.Height = toSplit.Bounds.Height - height - Padding;
            n2.SplitType = SplitType.Horizontal;

            if (n1.Bounds.Width > 0 && n1.Bounds.Height > 0)
                list.Add(n1);
            if (n2.Bounds.Width > 0 && n2.Bounds.Height > 0)
                list.Add(n2);
        }

        private TextureInfo FindBestFitForNode(Node node, List<TextureInfo> textures)
        {
            TextureInfo bestFit = null;

            float nodeArea = node.Bounds.Width * node.Bounds.Height;
            float maxCriteria = 0.0f;

            foreach (TextureInfo ti in textures)
            {
                switch (FitHeuristic)
                {
                    // Max of Width and Height ratios
                    case BestFitHeuristic.MaxOneAxis:
                        if (ti.Width <= node.Bounds.Width && ti.Height <= node.Bounds.Height)
                        {
                            float wRatio = (float)ti.Width / (float)node.Bounds.Width;
                            float hRatio = (float)ti.Height / (float)node.Bounds.Height;
                            float ratio = wRatio > hRatio ? wRatio : hRatio;
                            if (ratio > maxCriteria)
                            {
                                maxCriteria = ratio;
                                bestFit = ti;
                            }
                        }
                        break;

                    // Maximize Area coverage
                    case BestFitHeuristic.Area:

                        if (ti.Width <= node.Bounds.Width && ti.Height <= node.Bounds.Height)
                        {
                            float textureArea = ti.Width * ti.Height;
                            float coverage = textureArea / nodeArea;
                            if (coverage > maxCriteria)
                            {
                                maxCriteria = coverage;
                                bestFit = ti;
                            }
                        }
                        break;
                }
            }

            return bestFit;
        }

        private List<TextureInfo> LayoutAtlas(List<TextureInfo> inputTextures, Atlas atlas)
        {
            List<Node> freeList = new List<Node>();
            List<TextureInfo> textures = inputTextures.ToList();

            atlas.Nodes = new List<Node>();

            Node root = new Node();
            root.Bounds.Size = new Size(atlas.Width, atlas.Height);
            root.SplitType = SplitType.Horizontal;

            freeList.Add(root);

            while (freeList.Count > 0 && textures.Count > 0)
            {
                Node node = freeList[0];
                freeList.RemoveAt(0);

                TextureInfo bestFit = FindBestFitForNode(node, textures);
                if (bestFit != null)
                {
                    if (node.SplitType == SplitType.Horizontal)
                    {
                        HorizontalSplit(node, bestFit.Width, bestFit.Height, freeList);
                    }
                    else
                    {
                        VerticalSplit(node, bestFit.Width, bestFit.Height, freeList);
                    }

                    node.Texture = bestFit;
                    node.Bounds.Width = bestFit.Width;
                    node.Bounds.Height = bestFit.Height;

                    textures.Remove(bestFit);
                }

                atlas.Nodes.Add(node);
            }

            return textures;
        }
    }
}