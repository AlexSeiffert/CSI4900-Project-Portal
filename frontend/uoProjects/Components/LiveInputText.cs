using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace uoProjects.Components;

public class LiveInputText : InputText
{
    [Parameter] public EventCallback<string?> OnInput { get; set; }

    private string? typed;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "name", NameAttributeValue);
        builder.AddAttribute(3, "class", CssClass);
        builder.AddAttribute(4, "value", typed ?? CurrentValueAsString);
        builder.AddAttribute(5, "onchange", EventCallback.Factory.CreateBinder<string?>(this, value =>
        {
            typed = null;
            CurrentValueAsString = value;
        }, CurrentValueAsString));
        builder.AddAttribute(6, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
        {
            typed = e.Value as string;
            return OnInput.InvokeAsync(typed);
        }));
        builder.SetUpdatesAttributeName("value");
        builder.AddElementReferenceCapture(7, element => Element = element);
        builder.CloseElement();
    }
}
