// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

using System;
using System.Globalization;
using System.IO;
using Scientia.HtmlRenderer.Adapters;
using Scientia.HtmlRenderer.Core.Dom;
using Scientia.HtmlRenderer.Core.Entities;
using Scientia.HtmlRenderer.Core.Utils;

namespace Scientia.HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// Handle context menu.
    /// </summary>
    internal sealed class ContextMenuHandler : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// select all text
        /// </summary>
        private static readonly string SelectAll;

        /// <summary>
        /// copy selected text
        /// </summary>
        private static readonly string Copy;

        /// <summary>
        /// copy the link source
        /// </summary>
        private static readonly string CopyLink;

        /// <summary>
        /// open link (as left mouse click)
        /// </summary>
        private static readonly string OpenLink;

        /// <summary>
        /// copy the source of the image
        /// </summary>
        private static readonly string CopyImageLink;

        /// <summary>
        /// copy image to clipboard
        /// </summary>
        private static readonly string CopyImage;

        /// <summary>
        /// save image to disk
        /// </summary>
        private static readonly string SaveImage;

        /// <summary>
        /// open video in browser
        /// </summary>
        private static readonly string OpenVideo;

        /// <summary>
        /// copy video url to browser
        /// </summary>
        private static readonly string CopyVideoUrl;

        /// <summary>
        /// the selection handler linked to the context menu handler
        /// </summary>
        private readonly SelectionHandler SelectionHandler;

        /// <summary>
        /// the html container the handler is on
        /// </summary>
        private readonly HtmlContainerInt HtmlContainer;

        /// <summary>
        /// the last context menu shown
        /// </summary>
        private RContextMenu ContextMenu;

        /// <summary>
        /// the control that the context menu was shown on
        /// </summary>
        private RControl ParentControl;

        /// <summary>
        /// the css rectangle that context menu shown on
        /// </summary>
        private CssRect CurrentRect;

        /// <summary>
        /// the css link box that context menu shown on
        /// </summary>
        private CssBox CurrentLink;

        #endregion

        /// <summary>
        /// Init context menu items strings.
        /// </summary>
        static ContextMenuHandler()
        {
            if (CultureInfo.CurrentUICulture.Name.StartsWith("fr", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Tout sélectionner";
                Copy = "Copier";
                CopyLink = "Copier l'adresse du lien";
                OpenLink = "Ouvrir le lien";
                CopyImageLink = "Copier l'URL de l'image";
                CopyImage = "Copier l'image";
                SaveImage = "Enregistrer l'image sous...";
                OpenVideo = "Ouvrir la vidéo";
                CopyVideoUrl = "Copier l'URL de l'vidéo";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("de", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Alle auswählen";
                Copy = "Kopieren";
                CopyLink = "Link-Adresse kopieren";
                OpenLink = "Link öffnen";
                CopyImageLink = "Bild-URL kopieren";
                CopyImage = "Bild kopieren";
                SaveImage = "Bild speichern unter...";
                OpenVideo = "Video öffnen";
                CopyVideoUrl = "Video-URL kopieren";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("it", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Seleziona tutto";
                Copy = "Copia";
                CopyLink = "Copia indirizzo del link";
                OpenLink = "Apri link";
                CopyImageLink = "Copia URL immagine";
                CopyImage = "Copia immagine";
                SaveImage = "Salva immagine con nome...";
                OpenVideo = "Apri il video";
                CopyVideoUrl = "Copia URL video";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("es", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Seleccionar todo";
                Copy = "Copiar";
                CopyLink = "Copiar dirección de enlace";
                OpenLink = "Abrir enlace";
                CopyImageLink = "Copiar URL de la imagen";
                CopyImage = "Copiar imagen";
                SaveImage = "Guardar imagen como...";
                OpenVideo = "Abrir video";
                CopyVideoUrl = "Copiar URL de la video";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("ru", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Выбрать все";
                Copy = "Копировать";
                CopyLink = "Копировать адрес ссылки";
                OpenLink = "Перейти по ссылке";
                CopyImageLink = "Копировать адрес изображения";
                CopyImage = "Копировать изображение";
                SaveImage = "Сохранить изображение как...";
                OpenVideo = "Открыть видео";
                CopyVideoUrl = "Копировать адрес видео";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("sv", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Välj allt";
                Copy = "Kopiera";
                CopyLink = "Kopiera länkadress";
                OpenLink = "Öppna länk";
                CopyImageLink = "Kopiera bildens URL";
                CopyImage = "Kopiera bild";
                SaveImage = "Spara bild som...";
                OpenVideo = "Öppna video";
                CopyVideoUrl = "Kopiera video URL";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("hu", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Összes kiválasztása";
                Copy = "Másolás";
                CopyLink = "Hivatkozás címének másolása";
                OpenLink = "Hivatkozás megnyitása";
                CopyImageLink = "Kép URL másolása";
                CopyImage = "Kép másolása";
                SaveImage = "Kép mentése másként...";
                OpenVideo = "Videó megnyitása";
                CopyVideoUrl = "Videó URL másolása";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("cs", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Vybrat vše";
                Copy = "Kopírovat";
                CopyLink = "Kopírovat adresu odkazu";
                OpenLink = "Otevřít odkaz";
                CopyImageLink = "Kopírovat URL snímku";
                CopyImage = "Kopírovat snímek";
                SaveImage = "Uložit snímek jako...";
                OpenVideo = "Otevřít video";
                CopyVideoUrl = "Kopírovat URL video";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("da", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Vælg alt";
                Copy = "Kopiér";
                CopyLink = "Kopier link-adresse";
                OpenLink = "Åbn link";
                CopyImageLink = "Kopier billede-URL";
                CopyImage = "Kopier billede";
                SaveImage = "Gem billede som...";
                OpenVideo = "Åbn video";
                CopyVideoUrl = "Kopier video-URL";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("nl", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Alles selecteren";
                Copy = "Kopiëren";
                CopyLink = "Link adres kopiëren";
                OpenLink = "Link openen";
                CopyImageLink = "URL Afbeelding kopiëren";
                CopyImage = "Afbeelding kopiëren";
                SaveImage = "Bewaar afbeelding als...";
                OpenVideo = "Video openen";
                CopyVideoUrl = "URL video kopiëren";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("fi", StringComparison.InvariantCultureIgnoreCase))
            {
                SelectAll = "Valitse kaikki";
                Copy = "Kopioi";
                CopyLink = "Kopioi linkin osoite";
                OpenLink = "Avaa linkki";
                CopyImageLink = "Kopioi kuvan URL";
                CopyImage = "Kopioi kuva";
                SaveImage = "Tallena kuva nimellä...";
                OpenVideo = "Avaa video";
                CopyVideoUrl = "Kopioi video URL";
            }
            else
            {
                SelectAll = "Select all";
                Copy = "Copy";
                CopyLink = "Copy link address";
                OpenLink = "Open link";
                CopyImageLink = "Copy image URL";
                CopyImage = "Copy image";
                SaveImage = "Save image as...";
                OpenVideo = "Open video";
                CopyVideoUrl = "Copy video URL";
            }
        }

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="selectionHandler">the selection handler linked to the context menu handler</param>
        /// <param name="htmlContainer">the html container the handler is on</param>
        public ContextMenuHandler(SelectionHandler selectionHandler, HtmlContainerInt htmlContainer)
        {
            ArgChecker.AssertArgNotNull(selectionHandler, "selectionHandler");
            ArgChecker.AssertArgNotNull(htmlContainer, "htmlContainer");

            this.SelectionHandler = selectionHandler;
            this.HtmlContainer = htmlContainer;
        }

        /// <summary>
        /// Show context menu clicked on given rectangle.
        /// </summary>
        /// <param name="parent">the parent control to show the context menu on</param>
        /// <param name="rect">the rectangle that was clicked to show context menu</param>
        /// <param name="link">the link that was clicked to show context menu on</param>
        public void ShowContextMenu(RControl parent, CssRect rect, CssBox link)
        {
            try
            {
                this.DisposeContextMenu();

                this.ParentControl = parent;
                this.CurrentRect = rect;
                this.CurrentLink = link;
                this.ContextMenu = this.HtmlContainer.Adapter.GetContextMenu();

                if (rect != null)
                {
                    bool isVideo = false;
                    if (link != null)
                    {
                        isVideo = link is CssBoxFrame && ((CssBoxFrame)link).IsVideo;
                        var linkExist = !string.IsNullOrEmpty(link.HrefLink);
                        this.ContextMenu.AddItem(isVideo ? OpenVideo : OpenLink, linkExist, this.OnOpenLinkClick);
                        if (this.HtmlContainer.IsSelectionEnabled)
                        {
                            this.ContextMenu.AddItem(isVideo ? CopyVideoUrl : CopyLink, linkExist, this.OnCopyLinkClick);
                        }

                        this.ContextMenu.AddDivider();
                    }

                    if (rect.IsImage && !isVideo)
                    {
                        this.ContextMenu.AddItem(SaveImage, rect.Image != null, this.OnSaveImageClick);
                        if (this.HtmlContainer.IsSelectionEnabled)
                        {
                            this.ContextMenu.AddItem(CopyImageLink, !string.IsNullOrEmpty(this.CurrentRect.OwnerBox.GetAttribute("src")), this.OnCopyImageLinkClick);
                            this.ContextMenu.AddItem(CopyImage, rect.Image != null, this.OnCopyImageClick);
                        }

                        this.ContextMenu.AddDivider();
                    }

                    if (this.HtmlContainer.IsSelectionEnabled)
                    {
                        this.ContextMenu.AddItem(Copy, rect.Selected, this.OnCopyClick);
                    }
                }

                if (this.HtmlContainer.IsSelectionEnabled)
                {
                    this.ContextMenu.AddItem(SelectAll, true, this.OnSelectAllClick);
                }

                if (this.ContextMenu.ItemsCount > 0)
                {
                    this.ContextMenu.RemoveLastDivider();
                    this.ContextMenu.Show(parent, parent.MouseLocation);
                }
            }
            catch (Exception ex)
            {
                this.HtmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to show context menu", ex);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            this.DisposeContextMenu();
        }

        #region Private methods

        /// <summary>
        /// Dispose of the last used context menu.
        /// </summary>
        private void DisposeContextMenu()
        {
            try
            {
                if (this.ContextMenu != null)
                    this.ContextMenu.Dispose();
                this.ContextMenu = null;
                this.ParentControl = null;
                this.CurrentRect = null;
                this.CurrentLink = null;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Handle link click.
        /// </summary>
        private void OnOpenLinkClick(object sender, EventArgs eventArgs)
        {
            try
            {
                this.CurrentLink.HtmlContainer.HandleLinkClicked(this.ParentControl, this.ParentControl.MouseLocation, this.CurrentLink);
            }
            catch (HtmlLinkClickedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.HtmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to open link", ex);
            }
            finally
            {
                this.DisposeContextMenu();
            }
        }

        /// <summary>
        /// Copy the href of a link to clipboard.
        /// </summary>
        private void OnCopyLinkClick(object sender, EventArgs eventArgs)
        {
            try
            {
                this.HtmlContainer.Adapter.SetToClipboard(this.CurrentLink.HrefLink);
            }
            catch (Exception ex)
            {
                this.HtmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to copy link url to clipboard", ex);
            }
            finally
            {
                this.DisposeContextMenu();
            }
        }

        /// <summary>
        /// Open save as dialog to save the image
        /// </summary>
        private void OnSaveImageClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var imageSrc = this.CurrentRect.OwnerBox.GetAttribute("src");
                this.HtmlContainer.Adapter.SaveToFile(this.CurrentRect.Image, Path.GetFileName(imageSrc) ?? "image", Path.GetExtension(imageSrc) ?? "png");
            }
            catch (Exception ex)
            {
                this.HtmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to save image", ex);
            }
            finally
            {
                this.DisposeContextMenu();
            }
        }

        /// <summary>
        /// Copy the image source to clipboard.
        /// </summary>
        private void OnCopyImageLinkClick(object sender, EventArgs eventArgs)
        {
            try
            {
                this.HtmlContainer.Adapter.SetToClipboard(this.CurrentRect.OwnerBox.GetAttribute("src"));
            }
            catch (Exception ex)
            {
                this.HtmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to copy image url to clipboard", ex);
            }
            finally
            {
                this.DisposeContextMenu();
            }
        }

        /// <summary>
        /// Copy image object to clipboard.
        /// </summary>
        private void OnCopyImageClick(object sender, EventArgs eventArgs)
        {
            try
            {
                this.HtmlContainer.Adapter.SetToClipboard(this.CurrentRect.Image);
            }
            catch (Exception ex)
            {
                this.HtmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to copy image to clipboard", ex);
            }
            finally
            {
                this.DisposeContextMenu();
            }
        }

        /// <summary>
        /// Copy selected text.
        /// </summary>
        private void OnCopyClick(object sender, EventArgs eventArgs)
        {
            try
            {
                this.SelectionHandler.CopySelectedHtml();
            }
            catch (Exception ex)
            {
                this.HtmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to copy text to clipboard", ex);
            }
            finally
            {
                this.DisposeContextMenu();
            }
        }

        /// <summary>
        /// Select all text.
        /// </summary>
        private void OnSelectAllClick(object sender, EventArgs eventArgs)
        {
            try
            {
                this.SelectionHandler.SelectAll(this.ParentControl);
            }
            catch (Exception ex)
            {
                this.HtmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to select all text", ex);
            }
            finally
            {
                this.DisposeContextMenu();
            }
        }

        #endregion
    }
}