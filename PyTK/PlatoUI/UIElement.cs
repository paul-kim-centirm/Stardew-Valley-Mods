﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PyTK.PlatoUI
{
    public class UIElement
    {
        public static UIElement Viewportbase = UIElement.GetContainer("Viewport",0,UIHelper.GetViewport(),1);

        public virtual UIElement Base { get; set; } = null;
        public static UIElement DragElement { get; set; } = null;
        public virtual string Id { get; set; }
        public virtual List<string> Types { get; set; } = new List<string>();

        protected UIElement _parent = null;
        public virtual UIElement Parent
        {
            get
            {
                if (_parent == null && this != Viewportbase)
                    _parent = Base == null ? Viewportbase : Base;

                return _parent;
            }

            set
            {
                _parent = value;
            }

        }

        public virtual UIElement AttachedToElement { get; set; } = null;
        public virtual List<UIElement> Children { get; set; } = new List<UIElement>();
        public virtual int Z { get; set; } = 0;

        public virtual bool Disabled { get; set; } = false;

        public virtual bool Bordered { get; set; } = false;

        public virtual bool Overflow { get; set; } = true;

        public virtual bool OutOfBounds {
            get
            {
                return !Parent.Overflow && !Parent.OutOfBounds && !Parent.Bounds.Contains(Bounds);
            }
        }
        public virtual bool Tiled {
            get
            {
                return TileSize != -1;
            }
        }

        public virtual float TileScale
        {
            get
            {
                if (Theme != null)
                    return ((Theme.Width / 3f) / TileSize);
                else
                    return 1f;
            }
        }
        public virtual int TileSize { get; set; } = -1;

        public virtual Texture2D Theme { get; set; }



        public virtual Color Color { get; set; } = Color.White;

        public virtual float Opacity { get; set; } = 1f;

        public virtual int OffsetX { get; set; } = 0;

        public virtual int OffsetY { get; set; } = 0;

        public virtual int AddedWidth { get; set; } = 0;

        public virtual int AddedHeight { get; set; } = 0;

        public virtual bool IsContainer { get; set; } = false;

        public virtual Action<Point, bool, UIElement> HoverAction { get; set; } = null;

        public virtual Action<Point, bool, bool, bool, UIElement> ClickAction { get; set; } = null;

        public virtual Action<GameTime, UIElement> UpdateAction { get; set; } = null;

        public virtual Action<Keys, bool, UIElement> KeyAction { get; set; } = null;
        public virtual Action<int, UIElement> ScrollAction { get; set; } = null;

        public virtual Action<bool, UIElement> SelectAction { get; set; } = null;

        public virtual Func<bool, Point, UIElement, bool> DragAction { get; set; } = null;

        public virtual Func<UIElement, UIElement, Rectangle> Positioner { get; set; } = null;

        public virtual Rectangle? SourceRectangle { get; set; } = null;
        public virtual bool WasHover { get; set; } = false;

        public virtual bool Visible { get; set; } = true;
        public virtual bool IsSelected { get; set; } = false;

        public virtual bool IsSelectable { get; set; } = false;

        public virtual string SelectionId { get; set; } = "";

        public virtual bool IsDraggable { get; set; } = false;

        protected virtual Point? DragPosition { get; set; } = null;
        public virtual Vector2 DragPoint { get; set; } = Vector2.Zero;

        protected virtual Point? TempDragPoint { get; set; } = null;

        public virtual bool IsBeingDragged
        {
            get
            {
                return DragElement == this;
            }
        }

        protected Rectangle? _bounds;
        public virtual Rectangle Bounds
        {
            get
            {
                if (!_bounds.HasValue)
                    CalculateBounds();

                Rectangle r = new Rectangle(_bounds.Value.X + OffsetX, _bounds.Value.Y + OffsetY, _bounds.Value.Width + AddedWidth, _bounds.Value.Height + AddedHeight);

                if (IsBeingDragged && DragPosition.HasValue)
                    return new Rectangle(DragPosition.Value.X, DragPosition.Value.Y, r.Width, r.Height);
                return r;
            }
        }

        public UIElement(string id = "element", Func<UIElement, UIElement, Rectangle> positioner = null, int z = 0, Texture2D theme = null, Color? color = null, float opacity = 1f, bool container = false)
        {
            Id = id;

            if (!color.HasValue)
                color = Color.White;

            Theme = theme;
            Color = color.Value;
            IsContainer = container;
            Opacity = opacity;

            Z = z;

            if (positioner == null)
                Positioner = UIHelper.Fill;
            else
                Positioner = positioner;
        }

        public virtual UIElement WithBase(UIElement element)
        {
            Base = element;
            return this;
        }

        public virtual UIElement WithSourceRectangle(Rectangle sourceRectangle)
        {
            SourceRectangle = sourceRectangle;
            return this;
        }

        public virtual void Disable()
        {
            Disabled = true;

            foreach (UIElement child in Children)
                child.Disable();
        }
        public virtual void Enable()
        {
            Disabled = false;

            foreach (UIElement child in Children)
                child.Enable();
        }

        public virtual UIElement Clone(string id = null)
        {
            if (id == null)
                id = Id;

            var e = new UIElement(id, Positioner, Z, Theme, Color, Opacity, IsContainer).AsTiledBox(TileSize, Bordered).WithInteractivity(UpdateAction, HoverAction, ClickAction).WithTypes(Types.ToArray());
            e.Base = null;
            e.SelectAction = SelectAction;
            e.IsSelectable = IsSelectable;
            e.IsSelected = IsSelected;
            e.SelectionId = SelectionId;
            e.DragPoint = DragPoint;
            e.IsDraggable = IsDraggable;
            e.DragAction = DragAction;

            foreach (UIElement child in Children)
                e.Add(child.Clone());

            return e;
        }

        public virtual void Tranform(params object[] transformation)
        {
            int c = transformation.Count();
            for (int i = 0; i < c; i++)
            {
                if (i == 0 && transformation[i] != null)
                    OffsetX += UIHelper.GetAbs(transformation[i], Bounds.Width);

                if (i == 1 && transformation[i] != null)
                    OffsetY += UIHelper.GetAbs(transformation[i], Bounds.Height);

                if (i == 2 && transformation[i] != null)
                    AddedWidth += UIHelper.GetAbs(transformation[i], Bounds.Width);

                if (i == 3 && transformation[i] != null)
                    AddedHeight += UIHelper.GetAbs(transformation[i], Bounds.Height);
            }
        }

        public virtual void ResetTranformation()
        {
            OffsetX = 0;
            OffsetY = 0;
            AddedWidth = 0;
            AddedHeight = 0;
        }

        public virtual UIElement AsSelectable(string selctionId, Action<bool, UIElement> selectAction = null)
        {
            SelectAction = selectAction;
            SelectionId = selctionId;
            IsSelectable = true;
            return this;
        }

        public virtual UIElement AsDraggable(Func<bool, Point, UIElement, bool> dragAction = null, float dragX = 0.5f, float dragY = 0.5f)
        {
            IsDraggable = true;
            DragPoint = new Vector2(dragX, dragY);
            DragAction = dragAction;
            return this;
        }

        public virtual UIElement AsTiledBox(int tilesize, bool bordered)
        {
            TileSize = tilesize;
            Bordered = bordered;
            return this;
        }

        public virtual void AddTypes(params string[] types)
        {
            foreach (string type in Types)
                if (!Types.Contains(type))
                    Types.Add(type);
        }

        public virtual void RemoveTypes(params string[] types)
        {
            foreach (string type in Types)
                if (!Types.Contains(type))
                    Types.Remove(type);
        }

        public virtual UIElement WithTypes(params string[] types)
        {
            Types.AddRange(types);
            return this;
        }

        public virtual UIElement WithoutTypes()
        {
            Types.Clear();
            return this;
        }

        public virtual void PerformUpdate(GameTime time)
        {
            if (Disabled)
                return;

            UpdateAction?.Invoke(time, this);

            foreach (UIElement child in Children)
                child.PerformUpdate(time);
        }

        public virtual void PerformHover(Point point)
        {
            if (Disabled || OutOfBounds)
                return;

            if (HoverAction != null && Bounds.Contains(point))
            {
                HoverAction?.Invoke(point, true, this);
                WasHover = true;
            }
            else if (HoverAction != null && WasHover)
            {
                HoverAction?.Invoke(point, false, this);
                WasHover = false;
            }

            foreach (UIElement child in Children.Where(c => c.Visible))
                child.PerformHover(point);
        }

        public virtual void Deselect()
        {
            IsSelected = false;
            SelectAction?.Invoke(false, this);
        }

        public virtual void Select()
        {
            IsSelected = true;
            SelectAction?.Invoke(true, this);
        }

        public virtual void StopDrag(Point point)
        {
            if (IsBeingDragged)
            {
                DragAction?.Invoke(false, point, this);
                DragElement = null;
                DragPosition = null;
                TempDragPoint = null;
            }
        }

        public virtual void PerformClick(Point point, bool right, bool release, bool hold)
        {
            if (OutOfBounds || Disabled)
                return;

            if ((ClickAction != null || IsSelectable) && Bounds.Contains(point))
            {
                if (IsSelectable && !right && release)
                {
                    if (IsSelected)
                        Deselect();
                    else
                        Select();
                }

                if (IsDraggable && !right && hold && DragElement == null && DragAction != null && DragAction.Invoke(true, point, this) && !IsBeingDragged)
                {
                    DragElement = this;
                    DragPosition = null;
                    TempDragPoint = null;
                    if(DragPoint == Vector2.Zero)
                    {
                        var b = Bounds;
                        TempDragPoint = new Point(point.X - b.X, point.Y - b.Y);
                    }
                    PerformMouseMove(point);
                }

                if (IsBeingDragged && release && DragAction != null && DragAction.Invoke(false, point, this))
                {
                    DragPosition = null;
                    DragElement = null;
                    TempDragPoint = null;
                }

                ClickAction?.Invoke(point, right, release, hold, this);
            }

            foreach (UIElement child in Children.Where(c => c.Visible))
                child.PerformClick(point,right, release, hold);
        }

        public virtual void PerformMouseMove(Point point)
        {
            if (Disabled)
                return;

            if (IsBeingDragged)
            {
                DragPosition = null;
                var b = Bounds;
                Point p = TempDragPoint.HasValue ? TempDragPoint.Value : new Point(UIHelper.GetAbs(DragPoint.X, b.Width), UIHelper.GetAbs(DragPoint.Y, b.Height));
                DragPosition = new Point(point.X - p.X, point.Y - p.Y);
            }

            foreach (UIElement child in Children)
                child.PerformMouseMove(point);

            UpdateBounds();
        }

        public virtual void PerformKey(Keys key, bool released)
        {
            if (Disabled)
                return;

                KeyAction?.Invoke(key, released, this);

            foreach (UIElement child in Children)
                child.PerformKey(key, released);
        }

        public virtual void PerformScroll(int direction)
        {
            if (OutOfBounds || Disabled)
                return;

            ScrollAction?.Invoke(direction, this);

            foreach (UIElement child in Children)
                child.PerformScroll(direction);
        }
        
        public virtual UIElement WithInteractivity(Action<GameTime, UIElement> update = null, Action<Point, bool, UIElement> hover = null, Action<Point, bool,bool,bool, UIElement> click = null, Action<Keys,bool, UIElement> keys = null, Action<int,UIElement>scroll = null)
        {
            if(update != null)
                UpdateAction = update;

            if (hover != null)
                HoverAction = hover;

            if (click != null)
                ClickAction = click;

            if (keys != null)
                KeyAction = keys;

            if (scroll != null)
                ScrollAction = scroll;

            return this;
        }

        public virtual UIElement WithoutInteractivity(bool update = false, bool hover = false, bool click = false, bool keys = false, bool scroll = false)
        {
            UpdateAction = update ? null : UpdateAction;
            HoverAction = hover ? null :HoverAction;
            ClickAction = click ? null : ClickAction;
            KeyAction = keys ? null : KeyAction;
            ScrollAction = scroll ? null : ScrollAction;

            return this;
        }

        public virtual void CalculateBounds()
        {

            if (Positioner == null)
                Positioner = UIHelper.Fill;
            Rectangle b = Positioner(this, Parent);
            _bounds = b;

        }

        public virtual void UpdateBounds(bool children = true)
        {

            _bounds = null;
            CalculateBounds();

            if (children)
                foreach (UIElement child in Children)
                    child.UpdateBounds();
        }

        public virtual IEnumerable<UIElement> GetSelected(string selectionId = null)
        {
            if (IsSelected && (selectionId == null || selectionId == SelectionId))
                yield return this;

            foreach (UIElement child in Children)
                foreach (UIElement find in child.GetSelected(selectionId))
                    yield return find;
        }

        public virtual bool HasTypes(bool any, params string[] types)
        {
            bool hasType = true;

            foreach (string type in types)
            {
                if (!any)
                    hasType = Types.Contains(type) && hasType;
                else
                    hasType = Types.Contains(type) || hasType;
            }


            return hasType;
        }

        public virtual IEnumerable<UIElement> GetElementsByType(bool any, params string[] types)
        {
            if (HasTypes(any, types))
                yield return this;

            foreach (UIElement child in Children)
                foreach (UIElement find in child.GetElementsByType(any,types))
                    yield return find;
        }

        public virtual UIElement GetElementById(string id)
        {
            if (Id == id)
                return this;

            foreach (UIElement child in Children)
                if (child.GetElementById(id) is UIElement find)
                    return find;

            return null;
        }

        public static UIElement GetContainer(string id = "element", int z = 0, Func<UIElement, UIElement, Rectangle> positioner = null, float opacity = 1f)
        {
            return new UIElement(id, positioner,z,null,Color.White, opacity, container: true);
        }

        public static UIElement GetImage(Texture2D image, Color? color, string id = "element",float opacity = 1f,  int z = 0, Func<UIElement, UIElement, Rectangle> positioner = null)
        {
            return new UIElement(id, positioner, z, image, color, opacity);
        }

        public virtual void Add(UIElement element, bool disattach = true)
        {
            if(disattach)
                element.Disattach();
            element.Parent = this;
            if (element.Base == null)
                element.Base = Base;
            Children.Add(element);
        }

        public virtual void Remove(UIElement element)
        {
            Children.Remove(element);
        }

        public virtual void Disattach()
        {
            if(Parent != null)
                Parent.Children.Remove(this);
            Parent = null;
        }

        public virtual void Clear(UIElement element = null)
        {
            Children.Clear();
            if (element != null)
                Children.Add(element);
        }

       

    }
}