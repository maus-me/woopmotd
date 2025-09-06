using System;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace woopmotd.Gui;

public class GuiMotdDialog : GuiDialog
{
    private const string RichTextKey = "woopmotd:motd-rt";
    private const string ScrollKey = "woopmotd:motd-scroll";

    private readonly string _vtml;
    private readonly string _title;

    // Dynamic layout: computed from inset/container bounds after Compose
    private ElementBounds _clipBounds;
    private ElementBounds _contentBounds;
    private ElementBounds _scrollbarBounds;
    private const int ScrollbarWidth = 20;
    private const int ScrollbarGap = 6;

    public GuiMotdDialog(ICoreClientAPI capi, string titleLangCode, string vtml) : base(capi)
    {
        this._vtml = vtml;
        this._title = Lang.Get(titleLangCode) ?? "Message";
        SetupDialog();
    }

    private void SetupDialog()
    {
        int insetWidth = 600;
        int insetHeight = 500;
        int insetDepth = 3;
        
        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        
        // Bounds of main inset for scrolling content in the GUI
        ElementBounds insetBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, insetWidth, insetHeight);
        
        // Scrollbar to the right of inset (initialize before using in WithChildren)
        _scrollbarBounds = insetBounds.RightCopy().WithFixedWidth(ScrollbarWidth);
        _scrollbarBounds.fixedX += ScrollbarGap;

        ElementBounds bgBounds = ElementBounds.Fill
            .WithFixedPadding(GuiStyle.ElementToDialogPadding)
            .WithSizing(ElementSizing.FitToChildren)
            .WithChildren(insetBounds, _scrollbarBounds);

        // Inner area for text, leaving space for a gap and a scrollbar at the right
        _clipBounds = insetBounds.ForkContainingChild(
            GuiStyle.HalfPadding,
            GuiStyle.HalfPadding,
            GuiStyle.HalfPadding + ScrollbarGap + ScrollbarWidth,
            GuiStyle.HalfPadding
        );
        // Content container (rich text) starts at same X/Y as clip, full width of clip, initial height same as clip
        _contentBounds = ElementBounds.Fixed(_clipBounds.fixedX, _clipBounds.fixedY, _clipBounds.fixedWidth, _clipBounds.fixedHeight);
        // Scrollbar to the right of inset (already initialized above with gap)

        var okBtnBounds = ElementBounds.Fixed(0, 0, 100, 30).WithAlignment(EnumDialogArea.CenterBottom).WithFixedOffset(0, -10);

        var composer = capi.Gui.CreateCompo("woopmotd:motd", dialogBounds)
            .AddDialogBG(bgBounds, true)
            .AddDialogTitleBar(_title, OnTitleBarClose)
            .BeginChildElements(bgBounds)
                // Inset border and darkened background behind the text viewport
                .AddInset(insetBounds, insetDepth)
                // Clip region to constrain drawing of the rich text
                .BeginClip(_clipBounds)
                    .AddContainer(_contentBounds.FlatCopy(), "scroll-content")
                    .AddRichtext(_vtml, CairoFont.WhiteDetailText().WithLineHeightMultiplier(1.2), _contentBounds, RichTextKey)
                .EndClip()
                .AddVerticalScrollbar(OnScrollChanged, _scrollbarBounds, ScrollKey)
                .AddSmallButton(Lang.Get("woopmotd:motd-ok") ?? "OK", OnOkClicked, okBtnBounds)
            .EndChildElements();

        SingleComposer = composer.Compose();

        try
        {
            ConfigureScrollbar();
        }
        catch (Exception)
        {
            // If API differences occur, we avoid crashing the dialog; content will still show without scrolling.
        }
    }

    private void ConfigureScrollbar()
    {
        var sb = SingleComposer?.GetScrollbar(ScrollKey);
        var rt = SingleComposer?.GetRichtext(RichTextKey);
        if (sb == null || rt == null) return;

        // Measure content height (Bounds fixedHeight is populated after Compose)
        rt.Bounds.CalcWorldBounds();

        var contentHeight = (float)rt.Bounds.fixedHeight;
        var viewportHeight = (float)_clipBounds.fixedHeight;

        // Preferred API in VS 1.21
        try
        {
            sb.SetHeights(viewportHeight, contentHeight);
        }
        catch
        {
            // If SetHeights is unavailable in this VS version, the scrollbar may not function,
            // but the dialog will still render content without scrolling.
        }

        SetContentOffset(0);
    }

    private void OnScrollChanged(float value)
    {
        SetContentOffset(value);
    }

    private void SetContentOffset(float value)
    {
        var rt = SingleComposer?.GetRichtext(RichTextKey);
        if (rt == null) return;

        // Move the rich text inside the clipped viewport
        rt.Bounds.fixedY = _clipBounds.fixedY - value;
        rt.Bounds.CalcWorldBounds();
    }

    private bool OnOkClicked()
    {
        TryClose();
        return true;
    }

    private void OnTitleBarClose()
    {
        TryClose();
    }

    public override string ToggleKeyCombinationCode => null;

    // If you need to respond to screen size changes, call ConfigureScrollbar() from external triggers as needed.
}