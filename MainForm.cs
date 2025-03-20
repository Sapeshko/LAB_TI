using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TI_2;

public partial class MainForm : Form
{
    readonly StreamCipher streamCipher = new();
    public MainForm()
    {
        InitializeComponent();
    }

    void RegisterTextBox_TextChanged(object sender, EventArgs e)
    {
        LengthLabel.Text = $@"Длина введенных состояний: {RegisterTextBox.Text.Count(x => x is '0' or '1')}";
    }

    void ResultButton_Click(object sender, EventArgs e)
    {
        RegisterTextBox.Text = string.Join("", RegisterTextBox.Text.Where(x => x is '0' or '1'));
        if (RegisterTextBox.Text.Length != 35)
        {
            MessageBox.Show("Длина вашего регистра должна равняться 35 состояниям!", "Внимание");
            return;
        }

        if (PlainTextBox.Text.Length is 0)
        {
            MessageBox.Show("Выберите файл с вашим исходным текстом для шифрования/дешифрования!", "Внимание");
            return;
        }

        streamCipher.ProduceBitRegister(RegisterTextBox.Text);
        streamCipher.ProduceBitKey(streamCipher.PlainText.Length);
        KeyTextBox.Text = BitArrayToStr(streamCipher.BitKey);

        streamCipher.Cipher();
        CipherTextBox.Text = BitArrayToStr(streamCipher.CipherBit);
    }

    string BitArrayToStr(BitArray array)
    {
        StringBuilder temp = new();
        if (array.Length <= 240)
        {
            // Выводим биты в правильном порядке
            for (int i = 0; i < array.Length; i += 8)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (i + j < array.Length)
                    {
                        temp.Append(array[i + j] ? 1 : 0);
                    }
                }
            }
        }
        else
        {
            // Выводим первые 120 бит
            for (int i = 0; i < 120; i += 8)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (i + j < 120)
                    {
                        temp.Append(array[i + j] ? 1 : 0);
                    }
                }
            }
            temp.Append("...");

            // Выводим последние 120 бит
            for (int i = array.Length - 120; i < array.Length; i += 8)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (i + j < array.Length)
                    {
                        temp.Append(array[i + j] ? 1 : 0);
                    }
                }
            }
        }

        return temp.ToString();
    }

    void OpenFile_Click(object sender, EventArgs e)
    {
        if (OpenFileDialog.ShowDialog() != DialogResult.Cancel)
        {
            try
            {
                using (FileStream fileStream = new FileStream(OpenFileDialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[1024]; 
                    int bytesRead;
                    StringBuilder stringBuilder = new StringBuilder();

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {

                        BitArray bitArray = new BitArray(buffer.Take(bytesRead).ToArray());
                        for (int i = 0; i < bitArray.Length; i++)
                        {
                            stringBuilder.Append(bitArray[i] ? "1" : "0");
                        }
                    }

                    streamCipher.PlainText = new BitArray(stringBuilder.Length);
                    for (int i = 0; i < streamCipher.PlainText.Length; i++)
                    {
                        streamCipher.PlainText[i] = stringBuilder[i] == '1';
                    }

                    PlainTextBox.Text = BitArrayToStr(streamCipher.PlainText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка");
            }
        }
    }

    void SaveFile_Click(object sender, EventArgs e)
    {
        if (SaveFileDialog.ShowDialog() != DialogResult.Cancel)
        {
            using FileStream fileStream = new FileStream(SaveFileDialog.FileName, FileMode.Create);
            byte[] result = new byte[streamCipher.CipherBit.Count / 8];
            streamCipher.CipherBit.CopyTo(result, 0);
            fileStream.Write(result, 0, result.Length);
        }
    }

    private void MenuClear_Click(object sender, EventArgs e)
    {
        KeyTextBox.Clear();
        CipherTextBox.Clear();
        PlainTextBox.Clear();
    }

}