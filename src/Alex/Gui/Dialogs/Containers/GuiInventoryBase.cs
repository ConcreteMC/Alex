using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Gui.Elements.Inventory;
using Alex.Items;
using Alex.Utils;
using Alex.Utils.Inventories;
using Microsoft.Xna.Framework;
using RocketUI;
using GuiCursorEventArgs = Alex.API.Gui.Events.GuiCursorEventArgs;

namespace Alex.Gui.Dialogs.Containers
{
	public class GuiInventoryBase : GuiDialogBase
	{
		public EventHandler OnContainerClose;
		
		private GuiTextElement TextOverlay { get; }

		private GuiItem CursorItemRenderer { get; }
		public InventoryBase Inventory { get; }
		public GuiInventoryBase(InventoryBase inventory, GuiTextures background, int width, int height)
		{
			Inventory = inventory;
			
			ContentContainer.Background = background;
			ContentContainer.BackgroundOverlay = null;
            
			ContentContainer.Width = ContentContainer.MinWidth = ContentContainer.MaxWidth = width;
			ContentContainer.Height = ContentContainer.MinHeight = ContentContainer.MaxHeight = height;
            
			SetFixedSize(width, height);
            
			ContentContainer.AutoSizeMode = AutoSizeMode.None;
			
			AddChild(
				TextOverlay = new GuiTextElement(true)
				{
					HasShadow = true,
					Background = new Color(Color.Black, 0.35f),
					Enabled = false,
					FontStyle = FontStyle.DropShadow,
					TextColor = TextColor.Yellow,
					ClipToBounds = false,
					Anchor = Alignment.TopLeft
					//BackgroundOverlay = new Color(Color.Black, 0.35f),
				});
			
			AddChild(CursorItemRenderer = new GuiItem()
			{
				IsVisible = false,
				ClipToBounds = false,
				AutoSizeMode = AutoSizeMode.None,
				Height = 18,
				Width = 18,
				Anchor = Alignment.TopLeft
			});
			
			Inventory.SlotChanged += InventoryOnSlotChanged;
		}

		public void UpdateSlot(int inventoryId, int slotId, Item item)
		{
			var containerItem = ContentContainer.ChildElements
			   .Where(x => x is InventoryContainerItem).Cast<InventoryContainerItem>()
			   .FirstOrDefault(x => x.InventoryId == inventoryId && x.InventoryIndex == slotId);

			if (containerItem != null)
			{
				containerItem.Item = item;
			}
		}

		public InventoryContainerItem[] AddSlots(int x,
			int y,
			int itemsPerRow,
			int count,
			int firstSlotId,
			int inventoryId)
		{
			int                          columns = count / itemsPerRow;
			List<InventoryContainerItem> slots   = new List<InventoryContainerItem>();

			for (int col = 0; col < columns; col++)
			{
				int colOffset = col * 2;
				for (int row = 0; row < itemsPerRow; row++)
				{
					slots.Add(
						AddSlot(
							x + (row * (InventoryContainerItem.ItemWidth)), y + (col * (InventoryContainerItem.ItemWidth)),
							firstSlotId++, inventoryId));
				}


			}

			return slots.ToArray();
		}

		public InventoryContainerItem AddSlot(int x, int y, int slotId, int inventoryId)
		{
			var slot = CreateSlot(x, y, slotId, inventoryId);
			ContentContainer.AddChild(slot);

			return slot;
		}

		public InventoryContainerItem CreateSlot(int x, int y, int slotId, int inventoryId)
		{
			InventoryContainerItem containerItem = new InventoryContainerItem()
			{
				Margin = new Thickness(x, y, 0, 0),
				InventoryIndex = slotId,
				Anchor = Alignment.TopLeft,
				AutoSizeMode = AutoSizeMode.None,
				InventoryId = inventoryId,
				HighlightedBackground = Color.White * 0.8f
			};

			containerItem.CursorEnter += ContainerItemOnCursorEnter;
			containerItem.CursorLeave += ContainerItemOnCursorLeave;

			containerItem.CursorPressed += ContainerItemOnCursorPressed;

			return containerItem;
		}

		private void SetCursorItem(InventoryContainerItem slot)
		{
			if (slot.Item != null && slot.Item.Count > 0 && !(slot.Item is ItemAir))
			{
				CursorItemRenderer.IsVisible = true;
				CursorItemRenderer.Item = slot.Item;
				
				HoverItem = slot;
				SelectedItem = slot.Item;
				OnCursorItemChanged(slot, slot.Item);
				
				SetOverlayText(slot.Item);
					
				slot.Item = new ItemAir()
				{
					Count = 0
				};

				OnSlotChanged(slot, slot.Item);
				//OnItemSelected(slot, slot.Item);
			}
			else
			{
				CursorItemRenderer.IsVisible = false;
			}
		}

		private void ContainerItemOnCursorPressed(object sender, GuiCursorEventArgs e)
		{
			if (sender is InventoryContainerItem containerItem)
			{
				//We have not yet selected an item to move.
				if (HoverItem == null)
				{
					if (HighlightedSlot != null)
					{
						SetCursorItem(HighlightedSlot);
					}
				}
				else //We have already selected an item, drop item.
				{
					var originalHoverItem    = HoverItem;
					var originalHighlight    = HighlightedSlot;
					var originalSelectedItem = SelectedItem;

					HoverItem = null;
					SelectedItem = null;

					TextOverlay.IsVisible = false;
					CursorItemRenderer.IsVisible = false;
					
					OnSlotChanged(originalHighlight, originalSelectedItem);
					//OnItemDeSelected(originalHighlight, originalSelectedItem);

					if (HoverItem == HighlightedSlot) //We dropped the item in it's original slot.
					{
						originalHoverItem.Item = originalSelectedItem;
					}
					else if (containerItem.Item == null || containerItem.Item.Count == 0
					                                    || containerItem.Item is ItemAir) //Item dropped in empty slot.
					{
						containerItem.Item = originalSelectedItem;
					}
					else //Item was dropped on a slot that already has an item.
					{
						SetCursorItem(containerItem);

						containerItem.Item = originalSelectedItem;
					}

				}
			}
		}

		private Item SelectedItem { get; set; }

		private InventoryContainerItem HoverItem       { get; set; } = null;
		private InventoryContainerItem HighlightedSlot { get; set; } = null;

		private void ContainerItemOnCursorLeave(object sender, GuiCursorEventArgs e)
		{
			if (sender is InventoryContainerItem containerItem && containerItem == HighlightedSlot)
			{
				HighlightedSlot = null;

				if (SelectedItem != null)
				{
					SetOverlayText(SelectedItem);
				}
				else
				{
					SetOverlayText(null);
				}
			}
		}

		private void ContainerItemOnCursorEnter(object sender, GuiCursorEventArgs e)
		{
			if (sender is InventoryContainerItem containerItem)
			{
				HighlightedSlot = containerItem;
				SetOverlayText(containerItem.Item);
			}
		}

		private int _overlayStart = 0;
		private TimeSpan _nextUpdate = TimeSpan.MinValue;
		private bool _reverseMarqueue = false;
		private const int  _marqueueLength = 25;
		private string _overlayText = string.Empty;

		private void SetOverlayText(Item item)
		{
			if (item == null)
			{
				TextOverlay.IsVisible = false;
			}
			else
			{
				_overlayText = item?.GetDisplayName() ?? item.Name;
				_overlayStart = 0;
				_nextUpdate = TimeSpan.Zero;

				TextOverlay.IsVisible = true;
			}
		}
		
		private void Marqueue(GameTime gt)
		{
			if (_nextUpdate < gt.TotalGameTime)
			{
				string text = _overlayText;

				if (!string.IsNullOrWhiteSpace(text) && text.Length > _marqueueLength)
				{
					string overlayText = text.Substring(
						_overlayStart, Math.Min(_marqueueLength, Math.Max(0, text.Length - _overlayStart)));

					TextOverlay.Text = _reverseMarqueue ? $"...{overlayText}" : $"{overlayText}...";

					if (_reverseMarqueue)
					{
						_overlayStart--;

						if (_overlayStart <= 0)
						{
							_overlayStart = 0;
							_reverseMarqueue = false;
							_nextUpdate = gt.TotalGameTime + TimeSpan.FromMilliseconds(1500);

							return;
						}
					}
					else
					{
						_overlayStart++;
					}

					if (text.Length - _overlayStart <= _marqueueLength)
					{
						_overlayStart--;
						_reverseMarqueue = true;
						_nextUpdate = gt.TotalGameTime + TimeSpan.FromMilliseconds(1500);

						return;
					}
				}
				else if (text != null && text.Length < _marqueueLength)
				{
					TextOverlay.Text = text;
				}

				_nextUpdate = gt.TotalGameTime + TimeSpan.FromMilliseconds(500);
			}
		}

		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			var cursorListener = Alex.Instance.InputManager.CursorInputListener;
			
			var mousePos = cursorListener.GetCursorPosition();

			mousePos = Vector2.Transform(mousePos, Alex.Instance.GuiManager.ScaledResolution.InverseTransformMatrix);

			//TextOverlay.RenderPosition = mousePos;

			var point = mousePos.ToPoint();
			CursorItemRenderer.Margin = new Thickness(point.Y, point.X);
			TextOverlay.Margin = new Thickness(point.Y, point.X);
			
			Marqueue(gameTime);
			
			
			base.OnUpdate(gameTime);
		}

		private void InventoryOnSlotChanged(object sender, SlotChangedEventArgs e)
		{
			UpdateSlot(e.InventoryId, e.Index, e.Value);
		}

		/// <inheritdoc />
		public override void OnClose()
		{
			OnContainerClose?.Invoke(this, EventArgs.Empty);
			Inventory.SlotChanged -= InventoryOnSlotChanged;
			
			base.OnClose();
		}

		//protected virtual void OnItemSelected(InventoryContainerItem slot, Item item) { }

		protected virtual void OnSlotChanged(InventoryContainerItem slot, Item item) { }
		
		protected virtual void OnCursorItemChanged(InventoryContainerItem slot, Item item){}
	}
}