using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

class Program
{
    static readonly string NomeRegistro = "AlertaComputadorPublico";
    static readonly string NomeArquivo = "alerta.exe";
    static readonly string PastaDestino = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AlertaComputadorPublico");

    static List<string> mensagens = new List<string>()
    {
        "Não salve senhas no computadores.",
        "Sempre limpe o histórico e cookies após usar um PC público.",
        "Não acesse informações sensíveis em redes Wi-Fi públicas.",
        "Desconecte-se de todas as contas antes de sair.",
        "Não deixe seus dados salvos no navegador do computador.",
        "Evite entrar em contas importantes no computador.",
        "Use a navegação anônima para maior privacidade.",
        "Cuidado com sites suspeitos ou links desconhecidos."
    };

    static void Main(string[] args)
    {
        GarantirExecucaoRegistrada();

        while (true)
        {
            EnviarNotificacao();
            Thread.Sleep(TimeSpan.FromMinutes(10));
        }
    }

    static void GarantirExecucaoRegistrada()
    {
        string destinoFinal = Path.Combine(PastaDestino, NomeArquivo);
        string caminhoAtual = Process.GetCurrentProcess().MainModule.FileName;

        bool jaInstalado = string.Equals(
            Path.GetFullPath(destinoFinal),
            Path.GetFullPath(caminhoAtual),
            StringComparison.OrdinalIgnoreCase);

        if (!jaInstalado)
        {
            try
            {
                Directory.CreateDirectory(PastaDestino);
                File.Copy(caminhoAtual, destinoFinal, true);

                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key?.SetValue(NomeRegistro, $"\"{destinoFinal}\"");
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = destinoFinal,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                // Falha silenciosa (evita alertar o usuário)
            }
        }
    }

    static void EnviarNotificacao()
    {
        string tempImagePath = ExtrairImagemTemporaria("alerta_temp.jpg");

        string mensagem = mensagens[Random.Shared.Next(mensagens.Count)];

        new ToastContentBuilder()
            .AddHeroImage(new Uri("file:///" + tempImagePath))
            .AddText("Cuidado ao usar computadores públicos!")
            .AddText(mensagem)
            .AddToastActivationInfo(
                "https://www.gov.br/fundaj/pt-br/centrais-de-conteudo/noticias-1/12-seguranca-de-computadores",
                ToastActivationType.Protocol)
            .Show();
    }

    static string ExtrairImagemTemporaria(string fileName)
    {
        var imgBytes = AlertaComputadorPublico.Properties.Resources.alerta;

        string tempPath = Path.Combine(Path.GetTempPath(), fileName);

        File.WriteAllBytes(tempPath, imgBytes);

        return tempPath;
    }
}
