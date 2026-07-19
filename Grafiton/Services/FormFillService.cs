using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Grafiton.Models;
using PdfSharpCore.Pdf.AcroForms;
using PdfSharpCore.Pdf.IO;

namespace Grafiton.Services;

public class FormFillService : IFormFillService
{
    public Task<List<FormItemModel>> GetFormFieldsAsync(string pdfPath)
    {
        return Task.Run(() =>
        {
            var list = new List<FormItemModel>();
            if (!File.Exists(pdfPath)) return list;

            try
            {
                using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);
                var acroForm = document.AcroForm;

                if (acroForm != null && acroForm.Fields != null)
                {
                    foreach (string fieldName in acroForm.Fields.Names)
                    {
                        var field = acroForm.Fields[fieldName];
                        if (field == null) continue;

                        var item = new FormItemModel
                        {
                            Name = fieldName,
                            Value = field.Value?.ToString() ?? string.Empty
                        };

                        if (field is PdfTextField)
                        {
                            item.FieldType = FormFieldType.Text;
                        }
                        else if (field is PdfCheckBoxField cbField)
                        {
                            item.FieldType = FormFieldType.CheckBox;
                            item.IsChecked = cbField.Checked;
                        }
                        else if (field is PdfComboBoxField or PdfListBoxField)
                        {
                            item.FieldType = FormFieldType.Choice;
                        }

                        list.Add(item);
                    }
                }
            }
            catch
            {
                // Fallback for PDFs without AcroForm structure
            }

            return list;
        });
    }

    public Task FillFormFieldsAsync(string pdfPath, List<FormItemModel> fields, bool flatten, string outputPath)
    {
        return Task.Run(() =>
        {
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException("Le fichier PDF est introuvable.", pdfPath);

            using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Modify);
            var acroForm = document.AcroForm;

            if (acroForm != null && acroForm.Fields != null)
            {
                foreach (var item in fields)
                {
                    try
                    {
                        var field = acroForm.Fields[item.Name];
                        if (field == null) continue;

                        if (field is PdfTextField textField)
                        {
                            textField.Text = item.Value ?? string.Empty;
                        }
                        else if (field is PdfCheckBoxField cbField)
                        {
                            cbField.Checked = item.IsChecked;
                        }
                    }
                    catch
                    {
                        // Ignore individual field errors during bulk fill
                    }
                }

                if (flatten)
                {
                    // Set AcroForm fields as read-only to prevent editing
                    foreach (string fName in acroForm.Fields.Names)
                    {
                        var f = acroForm.Fields[fName];
                        if (f != null) f.ReadOnly = true;
                    }
                }
            }

            document.Save(outputPath);
        });
    }
}
