using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Utils;
using Alex.Gui.Elements.Inventory;
using Alex.Items;
using Alex.Utils;
using Alex.Utils.Inventories;
using Microsoft.Xna.Framework;
using RocketUI;
using GuiCursorEventArgs = Alex.API.Gui.Events.GuiCursorEventArgs;
using GuiCursorMoveEventArgs = Alex.API.Gui.Events.GuiCursorMoveEventArgs;

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
			Inventory.CursorChanged += InventoryOnCursorChanged;
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
			containerItem.CursorDown += ContainerItemOnCursorDown;
			containerItem.CursorUp += ContainerItemOnCursorUp;
			containerItem.CursorMove += ContainerItemOnCursorMove;

			return containerItem;
		}

		private void SetCursorItem(InventoryContainerItem slot, bool isServerTransaction, MouseButton button)
		{
			if (slot.Item != null && slot.Item.Count > 0 && !(slot.Item is ItemAir))
			{
				CursorItemRenderer.IsVisible = true;
				CursorItemRenderer.Item = slot.Item;
				
				HoverItem = slot;
				SelectedItem = slot.Item;
				OnCursorItemChanged(slot, slot.Item, isServerTransaction, button);
				
				SetOverlayText(slot.Item);
					
				slot.Item = new ItemAir()
				{
					Count = 0
				};

				OnSlotChanged(slot, slot.Item, isServerTransaction);
				//OnItemSelected(slot, slot.Item);
			}
			else
			{
				CursorItemRenderer.IsVisible = false;
				SelectedItem = null;
				
				OnCursorItemChanged(slot, slot.Item, isServerTransaction, button);
			}
		}

		private Dictionary<InventoryContainerItem, Item> _draggedContainers = new Dictionary<InventoryContainerItem, Item>();
		private InventoryContainerItem _dragStartItem = null;
		private bool _dragging = false;
		public static bool AllowDragging { get; set; } = false;
		
		private InventoryContainerItem _previousItem;

		private bool CanDrag()
		{
			return HoverItem.Item.Count > 0;
		}

		private void FinishDragging()
		{
			var dragStartItem = _dragStartItem;
			_dragStartItem = null;
			_dragging = false;
		}

		private void ContainerItemOnCursorMove(object sender, GuiCursorMoveEventArgs e)
		{
			if (!AllowDragging)
				return;
			
			if (sender is InventoryContainerItem containerItem)
			{
				if (!_dragging || containerItem == _previousItem || !CanDrag())
					return;

				_previousItem = containerItem;
				
				if (!_draggedContainers.ContainsKey(containerItem))
					_draggedContainers.Add(containerItem, containerItem.Item.Clone());
				
				if (containerItem.Item == null || containerItem.Item.Id == HoverItem.Item.Id &&
					containerItem.Item.Id < containerItem.Item.MaxStackSize)
				{
					if (containerItem.Item == null)
					{
						containerItem.Item = HoverItem.Item.Clone();
						containerItem.Item.Count = 1;
						HoverItem.Item.Count--;
					}
					else
					{
						containerItem.Item.Count += 1;
						HoverItem.Item.Count--;
					}
				}

				if (!CanDrag())
				{
					
				}
			}
		}
		
		private void ContainerItemOnCursorUp(object sender, GuiCursorEventArgs e)
		{
			if (!AllowDragging)
				return;
			
			if (sender is InventoryContainerItem containerItem)
			{
				if (_dragStartItem == null)
					return;

				if (_dragStartItem == containerItem)
					return;

				FinishDragging();
			}
		}

		private void ContainerItemOnCursorDown(object sender, GuiCursorEventArgs e)
		{
			if (!AllowDragging)
				return;
			
			if (sender is InventoryContainerItem containerItem)
			{
				if (_dragStartItem == null && (containerItem.Item == null || containerItem.Item is ItemAir || containerItem.Item.Id <= 0) && HoverItem != null)
				{
					_draggedContainers.Clear();
					_dragStartItem = containerItem;
					_dragging = true;
				}
			}
		}
		
		private void ContainerItemOnCursorPressed(object sender, GuiCursorEventArgs e)
		{
			if (sender is InventoryContainerItem containerItem)
			{
				//We have not yet selected an item to move.
				if (HoverItem == null)
				{
					if (HighlightedSlot != null && HighlightedSlot == containerItem)
					{
						SetCursorItem(HighlightedSlot, false, e.Button);
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
					
					OnSlotChanged(originalHighlight, originalSelectedItem, false);
					//OnItemDeSelected(originalHighlight, originalSelectedItem);

					if (HoverItem == HighlightedSlot) //We dropped the item in it's original slot.
					{
						SetCursorItem(originalHoverItem, false, e.Button);
						
						originalHoverItem.Item = originalSelectedItem;
					}
					else if (containerItem.Item == null || containerItem.Item.Count == 0
					                                    || containerItem.Item is ItemAir) //Item dropped in empty slot.
					{
						SetCursorItem(containerItem, false, e.Button);
						
						containerItem.Item = originalSelectedItem;
					}
					else //Item was dropped on a slot that already has an item.
					{
						SetCursorItem(containerItem, false, e.Button);

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
			if (!e.IsServerTransaction)
				return;
			
			UpdateSlot(e.InventoryId, e.Index, e.Value);
		}
		
		private void InventoryOnCursorChanged(object sender, SlotChangedEventArgs e)
		{
			if (!e.IsServerTransaction)
				return;

			SelectedItem = null;
			CursorItemRenderer.IsVisible = false;
		}

		/// <inheritdoc />
		public override void OnClose()
		{
			OnContainerClose?.Invoke(this, EventArgs.Empty);
			Inventory.SlotChanged -= InventoryOnSlotChanged;
			Inventory.CursorChanged -= InventoryOnCursorChanged;
			
			base.OnClose();
		}

		//protected virtual void OnItemSelected(InventoryContainerItem slot, Item item) { }

		protected virtual void OnSlotChanged(InventoryContainerItem slot, Item item, bool isServerTransaction) { }

		protected virtual void OnCursorItemChanged(InventoryContainerItem slot, Item item, bool isServerTransaction, MouseButton button)
		{
			if (isServerTransaction)
				return;
			
			Inventory.SetCursor(item, false, slot.InventoryIndex, button);
		}
	}
}