namespace Core.Utilities.Pdf;

public interface IPdfRenderer
{
    byte[] Render<TModel>(string templateKey, TModel model);
}
