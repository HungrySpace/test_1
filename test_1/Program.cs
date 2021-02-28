using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

namespace test_1
{
    class Program
    {
        static void Main(string[] args)
        {
            //токен для доступа к диску пользователя
            string token;
            Console.WriteLine($"client_id = 610e7cb79020423a8b684603d726fb53");
            token = InputProcessing("Введите token, например - AgAAAAAUHmUAAAbupw7Sl7yYN0diolNAqPExgJk : ");
            //имя папки на диске в которую будем заливать файлы
            string folderName;
            folderName = InputProcessing("Введите имя папки в которую хотите загрузить файлы: ");
            //путь откуда берем файлы для загрузки на диск
            string pathFolder;
            pathFolder = InputProcessing("Введите путь к папки в которой находятся файлы например - C:\\TestFolder : ");
            Task.Run(async () => {
                //ловим ошибки
                try
                {
                    var api = new DiskHttpApi(token);
                    //берем все элементы из корневой папки диска
                    var rootFolderData = await api.MetaInfo.GetInfoAsync(new ResourceRequest
                    {
                        //указываем откуда брать все файлы
                        Path = "/"
                    });
                    //Проверяем есть ли такая папка на диске (если тип равен директория то смотрим имя этой директории)
                    if (!rootFolderData.Embedded.Items.Any(i => i.Type == ResourceType.Dir && i.Name.Equals(folderName)))
                    {
                        //создаем директорию
                        await api.Commands.CreateDictionaryAsync("/" + folderName);
                    }
                    //собираем массив string (пути к файлом в этой дериктории с расширением jpg)
                    var files = Directory.GetFiles(pathFolder);
                    Console.WriteLine("Идет загрузка файлов ");
                    foreach (var file in files)
                    {
                        //GetFileName забирает имя файла из ссылки
                        string fileName = Path.GetFileName(file);
                        //формируем ссылку по которому будем заливать файл. overwrite индикатор перезаписи файлов
                        var link = await api.Files.GetUploadLinkAsync("/" + folderName + "/" + fileName, overwrite: false);
                        using (var fileStream = File.OpenRead(file))
                        {
                            //загружаем файлы на диск
                            await api.Files.UploadAsync(link, fileStream);
                        }
                        Console.WriteLine($"{fileName} загрузился");
                    }
                    Console.WriteLine("Загрузка завершена.");
                }
                //если поймали ошибку то выводим ее в консоль
                catch(Exception ex) { Console.WriteLine("Ошибка: " + ex.Message); }
            });
            Console.ReadLine();
        }
        //добиваемся от пользователя ввод данных
        static string InputProcessing(string massage)
        {
            string inputStr;
            do
            {
                Console.WriteLine(massage);
                inputStr = Console.ReadLine();
            } while (inputStr.Length < 1);
            return inputStr;
        }
    }
}
