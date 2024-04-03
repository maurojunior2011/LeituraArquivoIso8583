using System.Diagnostics.Metrics;
using System.Diagnostics;
using System;
using System.Net.NetworkInformation;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Numerics;
using System.Data.SqlTypes;

namespace Leitura;
class Program
{
    public static string ASCIIParaBinario(string texto)
    {
        string bin = string.Empty;
        for (int i = 0; i < texto.Length; ++i)
        {
            string cBin = Convert.ToString((int)texto[i], 2);
            cBin = cBin.PadLeft(8, '0');
            bin += cBin;
        }
        return bin;
    }

    static int tamanhoMensagem = 4;
    static int tamanhoBitmap = 8;

    static Dictionary<int, (string nome, int tamanho)> camposPadraoIso = new Dictionary<int, (string nome, int tamanho)>()
    {
        {03, ("Processing Code", 6)},
        {04, ("Transaction Amoaunt", 12)},
        {05, ("Settlement Amount", 12)},
        {07, ("Transmission Date & Time", 14)},
        {08, ("Fee Amount", 12)},
        {11, ("System Trace Audit Number", 6)},
        {12, ("Local Transaction Time", 6)},
        {15, ("Settlement Date", 4)},
        {19, ("Acquiring Country Code", 3)},
        {20, ("PAN Country Code", 3)},
        {23, ("PAN Sequence Number", 3)},
        {24, ("Function Code", 3)},
        {26, ("POS Capture Code", 2)},
        {32, ("Acquiring Identification Code", 1) },
        {35, ("Track 2", 56) }
    };

    enum Tipo
    {
        TipoA,
        TipoB
    }

    static void Main(string[] args)
    {
        RegistrarExtraEncodes();

        string bodyFile;

        using (StreamReader streamReader = new StreamReader("financial_transaction_message.dat", Encoding.GetEncoding(860)))
        {
            bodyFile = streamReader.ReadToEnd();
            List<string> dados = LerArquivo(bodyFile, Tipo.TipoA);
            foreach (string item in dados)
            {
                Console.WriteLine(item);
            }
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();

        using (StreamReader streamReader = new StreamReader("message_with_hex_bcd.dat", Encoding.GetEncoding(860)))
        {
            bodyFile = streamReader.ReadToEnd();
            List<string> dados = LerArquivo(bodyFile, Tipo.TipoB);
            foreach (string item in dados)
            {
                Console.WriteLine(item);
            }
        }
    }

    static void RegistrarExtraEncodes()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    static List<string> LerArquivo(string conteudo, Tipo tipo)
    {
        List<string> retorno = [];
        int tamanhoConteudo = conteudo.Length - 1;

        string mensagem = conteudo.Substring(0, tamanhoMensagem);
        retorno.Add("Message: " + mensagem);

        string bitmap = conteudo.Substring(tamanhoMensagem, tamanhoBitmap);
        string bitmapBinario = ASCIIParaBinario(bitmap);
        retorno.Add("Bitmap: " + bitmapBinario);

        List<int> campos = CamposNoArquivo(bitmapBinario);

        int inicioDoCampo = tamanhoMensagem + tamanhoBitmap;
        foreach (int item in campos)
        {
            bool existe = camposPadraoIso.TryGetValue(item, out (string nome, int tamanho) campo);

            if (existe && inicioDoCampo < tamanhoConteudo)
            {
                int tamanhoCampo = campo.tamanho;
                if (tipo == Tipo.TipoB && (item == 32 || item == 35))
                    tamanhoCampo = 0;
                retorno.Add($"{item} {campo.nome} ({tamanhoCampo}): {conteudo.Substring(inicioDoCampo, (inicioDoCampo + tamanhoCampo <= tamanhoConteudo ? tamanhoCampo : tamanhoConteudo - inicioDoCampo))}");
                inicioDoCampo += tamanhoCampo;
            }
        }

        return retorno;
    }

    static List<int> CamposNoArquivo(string bitmapBinario)
    {
        List<int> retorno = new List<int>();

        for (int i = 0; i < bitmapBinario.Length; i++)
        {
            if (bitmapBinario[i] == '1')
            {
                retorno.Add(i+1);
            }
        }

        return retorno;
    }
}