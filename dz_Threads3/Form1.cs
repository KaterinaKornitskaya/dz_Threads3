using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Remoting.Contexts;

namespace dz_Threads3
{
    public partial class Form1 : Form
    {
        public static SynchronizationContext uiContext;
        public Form1()
        {
            
            InitializeComponent();
            uiContext = SynchronizationContext.Current;
            label1.Text = "Введите расширение файла с точкой впереди " +
                "\n(например .txt)" ;
            label2.Text = "Введите параметр для поиска:" +
                "\n * если ищие любые файлы с выбранным расширением" +
                "\n ab? - если ищете определенные символы в названии";
            label3.Text = "Выберите диск из выпадающего списка:";
        }

        // обработчик нажатия кнопки Искать
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            string path;
            // путь - выбранный через элемент comboBox диск
            if (comboBoxDisk.SelectedIndex == 0)
                path = "C:\\";
            else
                path = "D:\\";
            // параметр для поиска - строка из двух textBox
            string ex;
            ex = textBoxSymbol.Text + textBoxExtension.Text;

            // создаем объект структуры и передаем параметры для поиска
            MyStruct myStruct = new MyStruct(path, ex);

            // создаем объем поток, передаем делегат с параметрами и 
            // инициализируем делегат методом Method
            Thread thread = new Thread(new ParameterizedThreadStart(Method));
            // объявляем поток фоновым
            thread.IsBackground = true;
            // запускаем поток, передаем в параметры структуру
            thread.Start(myStruct);
        }

        // структура для того, чтобы можно было в параметризованный делегат
        // передать две строки как один параметр
        struct MyStruct  
        {
            public string filePath;  // путь к файлу (диск)  
            public string symbol;    // расширение, символы для поиска
            public MyStruct(string f, string s)
            {
                filePath = f;
                symbol = s;
            }
        }

        // метод для того, что метод для поиска файла упаковать в такой,
        // какой подойдет для передачи в делегат ParameterizedThreadStart
        public void Method(object obj)
        {
            MyStruct myStruct = (MyStruct)obj;
            string path = myStruct.filePath;
            string extension = myStruct.symbol;
            // создаем список файлов типа FileInfo, инициализируем
            // аго результатом рекурсивного метода для поиска
            List<FileInfo> list = MyGetFiles(path, extension);

            // в цикле ждя каждого файла показываем папку, размер,
            // дату последнего обращения. 
            foreach (var item in list)
            {
                // здесь имя файла передаем как конструктор ListViewItem
                ListViewItem viewItem = new ListViewItem(item.Name);
                // остальные параметры вызываем через свойства
                viewItem.SubItems.Add(item.Directory.ToString());
                viewItem.SubItems.Add(item.Length.ToString());
                viewItem.SubItems.Add(item.LastAccessTime.ToString());
                // с помощью контекста синхронизации записываем все
                // в элемент listView
                uiContext.Send(x=> listView1.Items.Add(viewItem), null);
            }
        }

        // рекурсивынй метод для поиска файлов на диске компьютера
        public  List<FileInfo> MyGetFiles(string path, string extension)
        {
            List<FileInfo> all = new List<FileInfo>();
            try
            {
                foreach (var file in Directory.GetFiles(path, extension))
                {
                    all.Add(new FileInfo(file));
                }
                string[] directories = Directory.GetDirectories(path);
                if (directories.Length > 0)
                {
                    foreach (string directory in directories)
                    {
                        all.AddRange(MyGetFiles(directory, extension));
                    }
                }
            }
            catch { }
            return all;
        }
    }
}
