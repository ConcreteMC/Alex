using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Utilities;
using RocketUI;

namespace ResourcePackLib.ModelExplorer.Entities
{
    public class DrawableEntity : Entity, IDrawable
    {
        private bool _disposed;
        private int  _drawOrder;
        private bool _visible = true;

        /// <summary>
        /// Get the <see cref="P:Microsoft.Xna.Framework.DrawableGameComponent.GraphicsDevice" /> that this <see cref="T:Microsoft.Xna.Framework.DrawableGameComponent" /> uses for drawing.
        /// </summary>
        public GraphicsDevice GraphicsDevice => this.Game.GraphicsDevice;

        public int DrawOrder
        {
            get => this._drawOrder;
            set
            {
                if (this._drawOrder == value)
                    return;
                this._drawOrder = value;
                this.OnDrawOrderChanged((object) this, EventArgs.Empty);
            }
        }

        public bool Visible
        {
            get => this._visible;
            set
            {
                if (this._visible == value)
                    return;
                this._visible = value;
                this.OnVisibleChanged((object) this, EventArgs.Empty);
            }
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> DrawOrderChanged;

        /// <inheritdoc />
        public event EventHandler<EventArgs> VisibleChanged;

        protected Model Model { get; set; }

        public DrawableEntity(IGame game) : base(game)
        {
        }

        public override void Initialize()
        {
            if (Initialized)
                return;
            base.Initialize();
            this.LoadContent();
        }

        protected override void Dispose(bool disposing)
        {
            if (this._disposed)
                return;
            this._disposed = true;
            this.UnloadContent();
        }

        /// <summary>Load graphical resources needed by this component.</summary>
        protected virtual void LoadContent()
        {
        }

        /// <summary>Unload graphical resources needed by this component.</summary>
        protected virtual void UnloadContent()
        {
        }

        /// <summary>Draw this component.</summary>
        /// <param name="gameTime">The time elapsed since the last call to <see cref="M:Microsoft.Xna.Framework.DrawableGameComponent.Draw(Microsoft.Xna.Framework.GameTime)" />.</param>
        public virtual void Draw(GameTime gameTime)
        {
            if (!Visible) return;

            DrawModel(Model);
        }

        protected void DrawModel(Model model)
        {
            if (model != null)
            {
                using (GraphicsContext.CreateContext(GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.Default, RasterizerState.CullNone))
                {
                    var camera = ((IGame) Game).Camera;
                    model.Draw(Transform.World, camera.View, camera.Projection);
                }
            }
        }

        /// <summary>
        /// Called when <see cref="P:Microsoft.Xna.Framework.DrawableGameComponent.Visible" /> changed.
        /// </summary>
        /// <param name="sender">This <see cref="T:Microsoft.Xna.Framework.DrawableGameComponent" />.</param>
        /// <param name="args">Arguments to the <see cref="E:Microsoft.Xna.Framework.DrawableGameComponent.VisibleChanged" /> event.</param>
        protected virtual void OnVisibleChanged(object sender, EventArgs args) =>
            EventHelpers.Raise<EventArgs>(sender, this.VisibleChanged, args);

        /// <summary>
        /// Called when <see cref="P:Microsoft.Xna.Framework.DrawableGameComponent.DrawOrder" /> changed.
        /// </summary>
        /// <param name="sender">This <see cref="T:Microsoft.Xna.Framework.DrawableGameComponent" />.</param>
        /// <param name="args">Arguments to the <see cref="E:Microsoft.Xna.Framework.DrawableGameComponent.DrawOrderChanged" /> event.</param>
        protected virtual void OnDrawOrderChanged(object sender, EventArgs args) =>
            EventHelpers.Raise<EventArgs>(sender, this.DrawOrderChanged, args);
    }
}