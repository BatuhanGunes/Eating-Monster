using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace EatingMonster
{
    public partial class GameForm : Form
    {

        //Dusman Degiskenleri
        readonly List<Dusman> Dusman; //butun dusmanlari tutar.
        int DusmanSirasi = 0;
        readonly int DusmanHizi = 250; //Düşmanları bir sonraki hareketine kadar bekleyeceği süre (milisaniye)ne kadar artarsa o kadar yavaşlar.
        readonly int DusmanSalmaHizi = 5000; //Bir sonraki düşmanın harekete geçmeden önce beklemesi gereken süre (milisaniye)

        //Harita Degiskenleri
        readonly string Harita_dosya = Application.StartupPath + "\\Map.map"; //Harita dosyasinin konumu
        readonly int Harita_buyuklugu = 20; //Haritanın buyuklugunu tutacak degisken
        readonly int[,,] Harita; //Haritayı 3 boyutlu Array formatinda olusturuyoruz (x, y, z)
        readonly int KareBuyuklugu = 32; //Duvarlarin piksel buyuklugu 
        int YemSayac = 0;
        readonly int YemPuani = 10;

        //Oyuncu Degiskenleri 
        int Puan = 0; //Puani tutacak degisken
        int playerX = 0; //Oyuncunun X kordinatini tutacak degisken
        int playerY = 0; //Oyuncunun Y kordinatini tutacak degisken
        int Yon_bilgisi = 0; //Oyuncunun Hareket yonu 
        int Agiz = 0; //Karakterin animasyon hareketini tutacak degisken
        int Can = 3; //Karakterin kac cani oldugunu tutacak degisken 
        readonly int KarakterHizi = 150; //Karakterin bir sonraki hareketine kadar bekleyeceği süre (milisaniye)ne kadar artarsa o kadar yavaşlar.

        //Grafik Degiskenleri
        Graphics BackBuffer; //double buffer (draws the frame)
        Graphics Buffer; //Ekranda cerceveyi olusturur
        Bitmap Ekran; //Oyunun olusacagi cerceve boyutlarini tutacak 
        Rectangle rctDestination; //Resim ve Kareleri cizerken kulanacagiz. 
        readonly Bitmap Yem;

        public GameForm()    
        {
            InitializeComponent(); //Formu ve tum degiskenleri hazirlar

            //Formun ekrani ortalamasini Saglayan kod
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            //Haritayi Olusturur (bos)
            Harita = new int[Harita_buyuklugu, Harita_buyuklugu, 10];

            //Haritayi yukler 
            OyunuYukle(Harita_dosya);

            /*
             * Bu bolumde Dusmanlari ekliyecegiz 
             * 1. İlk olarak Dusmanları olusturacagız.
             * 2. Dusmanların rekleri degistirilecek 
             * 3. Dusman goruntuleri degistirilecek 
             * 4. Olusturulan dusmanlar dusman listesine eklenicek
             */

            //Dusmanlarin bulunacagi listeyi olusturmak icin
            Dusman = new List<Dusman>();

            //Kirmizi Dusman 
            Dusman d1 = new Dusman
            {
                Renk = Brushes.Red,
                img = Properties.Resources.red
            };
            Dusman.Add(d1);

            //Mavi Dusman
            Dusman d2 = new Dusman
            {
                Renk = Brushes.Blue,
                img = Properties.Resources.blue
            };
            Dusman.Add(d2);

            //Pembe Dusman
            Dusman d3 = new Dusman
            {
                Renk = Brushes.Pink,
                img = Properties.Resources.pink
            };
            Dusman.Add(d3);

            //Yem resimlerini degistirmek , ve siyahi seffaf yapmak icin (Ortusme icin)
            Yem = Properties.Resources.food;
            Yem.MakeTransparent(Color.Black);

            KonumSifirlama();
        }

        public void YemleriSay()
        {
            for (int x = 0; x < Harita_buyuklugu; x++)
            {
                for (int y = 0; y < Harita_buyuklugu; y++)
                {
                    if (Harita[x, y, 1] == 1)
                    {
                        YemSayac++;
                    }
                }
            }
        }

        /// Oyun penceresi gosterilmeden bu method yuklenir.
        private void GameForm_Load(object sender, EventArgs e)
        {
            this.Show(); //Formu gosterir. 
            this.Focus(); //Forma odaklanir.

            //Formun boyutlarının haritaya tam olarak denk gelmesi için gereken hesaplamalar
            //birimlerin piksel boyutlari * birim sayisi
            this.Height = 679;
            this.Width = 656;

            YemleriSay();

            //Oyunu baslatan komutu cagırır. 
            OyunuBaslat();
        }

        /// Form kapandiginda gerceklesmesi gerekenler
        void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            //Uygulamanin calisma sürecini oldurur
            //bunun amacı uygulamadaki sonsuz dongulerdir 
            //uygulama calismaya devam ettikce sonsuz döngülerde devam edicektir, 
            //Bunu yapmazsak, surec devam edicek dongulerimiz bitmeyecektir.
        }

        /// Bu methodta fare formun üzerine geldiginde gerceklesicek olaylar mevcuttur 
        void GameForm_MouseMove(object sender, MouseEventArgs e)
        {
            //Sol tık yapildigında tıklanan kareye duvar ekler yani 2 yapar.
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //fare ilgili kareyi bulur 
                int mouseX = e.X / KareBuyuklugu; //Karenin X kordinatini alır. 
                int mouseY = e.Y / KareBuyuklugu; //Karenin Y kordinatini alır.
                Harita[mouseX, mouseY, 1] = 2;
                //Harita üzerinde isaretlenen yere duvar ekler
            }
            //sag tık yapıldığında tıklanan karedeyi siler yani 0 yapar.
            else if (e.Button == System.Windows.Forms.MouseButtons.Right) 
            {
                //fare duvarlari bulur 
                int mouseX = e.X / KareBuyuklugu; //Karenin X kordinatini alır. 
                int mouseY = e.Y / KareBuyuklugu; //Karenin Y kordinatini alır. 
                Harita[mouseX, mouseY, 1] = 0; //Harita üzerinde isaretlenen yeri siler
                Harita[playerX, playerY, 1] = 0;
            }
            //Farenin orta tuşuna tıklandiginda ilgili kareye yem ekler yani 1 yapar.
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle) 
            {
                int mouseX = e.X / KareBuyuklugu; //Karenin X kordinatini alır.
                int mouseY = e.Y / KareBuyuklugu; //Karenin Y kordinatini alır.
                Harita[mouseX, mouseY, 1] = 1; //Harita üzerinde isaretlenen yere yem ekler
            }
        }

        void GameForm_MouseClick(object sender, MouseEventArgs e)
        {
            //Sol tık yapildigında tıklanan kareye duvar ekler yani 2 yapar.
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //fare ilgili kareyi bulur 
                int mouseX = e.X / KareBuyuklugu; //Karenin X kordinatini alır. 
                int mouseY = e.Y / KareBuyuklugu; //Karenin Y kordinatini alır.
                Harita[mouseX, mouseY, 1] = 2;
                //Harita üzerinde isaretlenen yere duvar ekler
            }
            //sag tık yapıldığında tıklanan karedeyi siler yani 0 yapar.
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //fare duvarlari bulur 
                int mouseX = e.X / KareBuyuklugu; //Karenin X kordinatini alır. 
                int mouseY = e.Y / KareBuyuklugu; //Karenin Y kordinatini alır. 
                Harita[mouseX, mouseY, 1] = 0; //Harita üzerinde isaretlenen yeri siler
                Harita[playerX, playerY, 1] = 0;
            }
            //Farenin orta tuşuna tıklandiginda ilgili kareye yem ekler yani 1 yapar.
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                int mouseX = e.X / KareBuyuklugu; //Karenin X kordinatini alır.
                int mouseY = e.Y / KareBuyuklugu; //Karenin Y kordinatini alır.
                Harita[mouseX, mouseY, 1] = 1; //Harita üzerinde isaretlenen yere yem ekler
            }
        }

        /// bu method bir tusa basildiginda gerceklesicek
        void GameForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //switch-case yöntemi ile klavyeden hangi tusa basildigini kontrol ediyoruz.
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.A: //sola hareket 
                    if (Harita[playerX - 1, playerY, 1] != 2) //Duvar icine girip girmedigini kontrol eder
                    {
                        Yon_bilgisi = 3; //Karakterin tasinacagi yeni konumun yonunu belirler
                    }
                    break;
                case System.Windows.Forms.Keys.S: //Asagıya hareket 
                    if (Harita[playerX, playerY + 1, 1] != 2) //Duvar icine girip girmedigini kontrol eder
                    {
                        Yon_bilgisi = 2; //Karakterin tasinacagi yeni yonu belirler 
                    }
                    break;
                case System.Windows.Forms.Keys.D: //saga Haraket 
                    if (Harita[playerX + 1, playerY, 1] != 2) //Duvar icine girip girmedigini kontrol eder
                    {
                        Yon_bilgisi = 4; //Karakterin tasinacagi yeni yonu belirler 
                    }
                    break;
                case System.Windows.Forms.Keys.W: //Yukariya hareket 
                    if (Harita[playerX, playerY - 1, 1] != 2) //Duvar icine girip girmedigini kontrol eder
                    {
                        Yon_bilgisi = 1; //Karakterin tasinacagi yeni yonu belirler
                    }
                    break;
                case System.Windows.Forms.Keys.Q: //Hareketi durdurmak icin   
                    Yon_bilgisi = 0; // karakteri olduğu konumda durmasını sağlar.
                    break;
                case Keys.F1: //Harita dosyasını kaydeder.
                    OyunuKaydet(Harita_dosya);
                    break;
                case Keys.F2: //Harita dosyasını yükler.
                    OyunuYukle(Harita_dosya);
                    break;
            }
        }

        /// Asagidaki method butun dusmanlari ve oyuncunun pozisyonlarini sifirlar.
        public void KonumSifirlama()
        {
            //Butun dusmanlari bu dongu icerisinde sifirlar
            for (int i = 0; i < Dusman.Count; i++)
            {
                Dusman[i].DusmanX = (Harita_buyuklugu / 2) - 1; //default X 
                Dusman[i].DusmanY = (Harita_buyuklugu / 2) - 1; //Default Y 
            }

            //Karakterin pozisyonunu ayarlar
            playerX = 9;
            playerY = Harita_buyuklugu - 5;
        }

        /// Bu metod tüm grafik değişkenlerini oluşturur ve ana oyun döngüsüne başlar
        public void OyunuBaslat()
        {
            Buffer = this.CreateGraphics(); //grafik yapimizi ekrana çekecek
            Ekran = new System.Drawing.Bitmap(this.Height, this.Width); //Yapimiz
            BackBuffer = Graphics.FromImage(Ekran); //Ekrani olusturan kod
            OyunDongusu(); //Oyun döngüsünü baslatan methodu cagiracak
        }


        /// Oyun oynanirken bu method bir dongu olarak calisacak
        public void OyunDongusu()
        {
            DusmanSirasi = Dusman.Count;

            new Thread(() =>
            {
                //Dusmanlarin farklı zamanlarda harekete geçmesi icin
                while (DusmanSirasi != 0) //Tum dusmanlari birakana dek devam edicek
                {
                    DusmanSirasi--; //Dusmani serbest birakir
                    Thread.Sleep(DusmanSalmaHizi); //Bir diger dusmani bırakmak icin 1000mS=10sn bekler
                }
            }).Start(); //Yeni bir thread baslatir.

            /* 
             * Bu thread butun dusmanlari ayni anda hareket ettirecek
             * bu döngü yukarıda belirtilen DusmanHızı adlı değişkenin değerine göre
             * tekrarlanicak yani "DusmanHızı" milisaniye boyunca uyku halinde kalicak
             */
            new Thread(() =>
            {
                while (true) //Dusmanlari sonsuza dek hareket ettiricek
                {
                    DusmanlarinUstUsteGelmeDurumunuKontrolEt(); //Dusmanlarin ortusmesini durdurur 
                    DusmanHareket();
                    Thread.Sleep(DusmanHizi); //Tekrar taşımadan önce Dusmanlari bekletir.
                    
                }
            }).Start(); //Thread baslatir.

            /*
             * Karakter döngüsü
             * Oyuncunun Hareketleri + Animasyon Ayarları, can azalması
             * Ayni dusmanlar gibi calisacak, sadece yonlendirilebilecek
             */
            new Thread(() =>
            {
                while (true) //Sonsuz dongu
                {
                    if (Agiz == 1) //Agiz Acık ise
                        Agiz = 0; //Kapat
                    else //degilse 
                        Agiz = 1; //Ac

                    OyuncuHareket(); //Oyuncuyu hareket ettirmek için gerekli işlemler gerçekleştirilir.

                    if (Harita[playerX, playerY, 1] == 1) //Mevcut zemin eger 1re esit ise yiyebilecegi yem var
                    {
                        Puan += 10; //Yem yediginde puan degiskenini arttirir.
                        Harita[playerX, playerY, 1] = 0; //Yemi harita uzerinden kaldirir.
                    }

                    //Oyuncunun ölü olup olmadigini kontrol eder
                    for (int i = 0; i < Dusman.Count; i++) //Bu dongu butun dusmanlar icin gecerli olacaktir.
                    {
                        if (Dusman[i].DusmanX == playerX && Dusman[i].DusmanY == playerY) //Dusman ile karakterin kesisme durumunu kontrol eder. 
                        {
                            Can--; //Bir adet can silinir. 
                            Thread.Sleep(2000); //Karakter oldukten sonra 2000ms bekletir.

                            //Oyuncu bir canını kaybetti, Bu yuzden butun konumlar sıfırlanıcak
                            KonumSifirlama();
                            Yon_bilgisi = 0;
                            Agiz = 0;
                        }
                    }
                    //Bu donguyu 150ms de bir calistirir.
                    Thread.Sleep(KarakterHizi);
                }
            }).Start();

            //Gercek Oyun dongusu, oyunun bitme durumlarını kontrol eder.
            OyunBittiMi();
        }

        public void OyunBittiMi()
        {
            while (true)
            {
                //Bir windows form çalıştırılığında, döngü içerisinde gerçekleştirilmek istenen olaylar bir kuyruğa alınır
                //ve bu kuyrukta bekletilir. Olayların kuyrukta bekletiliyor olması,  uygulamanın cevap vermemesine yol acar
                //Buda uygulamada hatalara sebep olabilir bunu onlemek icin Application.DoEvents() kullanılır.
                Application.DoEvents();

                //Oyuncu butun yemleri topladiginda oyunu bitirir.
                if (Puan >= (YemSayac * YemPuani))
                {
                    GameForm.ActiveForm.Hide();
                    WinForm frm = new WinForm();
                    frm.ShowDialog();
                    frm.Dispose();

                    //butun islemleri bitirir.//Programi ve butun sonsuz donguleri sonlandirir.
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }

                else if (Can == 0)
                {
                    GameForm.ActiveForm.Hide();
                    LoseForm frm = new LoseForm();
                    frm.ShowDialog();
                    frm.Dispose();

                    //butun islemleri bitirir.//Programi ve butun sonsuz donguleri sonlandirir.
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                //Oyunu olustur methodunu cagiralim
                OyunuOlustur();
            }
        }

        public void DusmanHareket()
        {
            //teker teker dusmanlar dongu icine giricek
            for (int i = 0; i < Dusman.Count - DusmanSirasi; i++)
            {
                if (Dusman[i].Hareket) //bir dusman ortusmuyorsa bir sonraki konuma gecicek
                {
                    //Dusmanlarin Oyuncuya gore hareket etmesini saglayan kodlar 
                    if (Dusman[i].DusmanY > playerY && Harita[Dusman[i].DusmanX, Dusman[i].DusmanY - 1, 1] != 2)
                    //Oyuncu Dusmana gore yukarıda ise dusmani bir kare yukari tasır 
                    {
                        Dusman[i].DusmanY--; //Dusmani bir kare yukari tasır
                    }
                    else if (Dusman[i].DusmanY < playerY && Harita[Dusman[i].DusmanX, Dusman[i].DusmanY + 1, 1] != 2 && Dusman[i].DusmanX != (Harita_buyuklugu / 2) - 1)
                    //Oyuncu Dusmana gore asagida ise dusmani bir kare asagiya tasır 
                    //(Ayrica bu if blogunun son kismi dusmanin baslangictaki konumuna donmesini onler)
                    {
                        Dusman[i].DusmanY++; //Dusmani bir kare asagiya tasir 
                    }
                    else if (Dusman[i].DusmanX > playerX && Harita[Dusman[i].DusmanX - 1, Dusman[i].DusmanY, 1] != 2)
                    //Oyuncu Dusmana gore solda ise dusmani bir kare sola tasır 
                    {
                        Dusman[i].DusmanX--; //Dusmani bir kare sola tasır. 
                    }
                    else if (Dusman[i].DusmanX < playerX && Harita[Dusman[i].DusmanX + 1, Dusman[i].DusmanY, 1] != 2)
                    //Oyuncu Dusmana gore sagda ise dusmani bir kare saga tasır 
                    {
                        Dusman[i].DusmanX++; //Dusmani bir kare saga tasır.
                    }
                    else //Eger dusman oyuncuya dogru ilerleyemez ise yinede baska bir yone hareket eder ( Not : zaman zaman takılmalar meydana geliyor.)
                    {
                        if (Dusman[i].DusmanY - 1 != -1) //Dusmanin sınırlardan disari cikip cikmadigini kontrol eder.
                        {
                            if (Harita[Dusman[i].DusmanX, Dusman[i].DusmanY - 1, 1] != 2) //Dusmanin yukari cıkabilirligini kontrol eder. 
                            {
                                Dusman[i].DusmanY--; //Yukari tasır.
                            }
                        }
                        else if (Dusman[i].DusmanX + 1 != Harita_buyuklugu) //Dusmanin sınırlardan disari cikip cikmadigini kontrol eder.
                        {
                            if (Harita[Dusman[i].DusmanX + 1, Dusman[i].DusmanY + 1, 1] != 2) //Dusmanin saga gidebilirligini kontrol eder.
                            {
                                Dusman[i].DusmanX++; //Saga tasır
                            }
                        }
                        else if (Dusman[i].DusmanX - 1 != -1)//Dusmanin sınırlardan disari cikip cikmadigini kontrol eder
                        {
                            if (Harita[Dusman[i].DusmanX - 1, Dusman[i].DusmanY, 1] != 2) //Dusmanin sola gidebilirligini kontrol eder.
                            {
                                Dusman[i].DusmanX--; //Sola tasır
                            }
                        }
                        else if (Dusman[i].DusmanY + 1 != Harita_buyuklugu && Dusman[i].DusmanX != (Harita_buyuklugu / 2) - 1) //Dusmanin sınırlardan disari cikip cikmadigini kontrol eder 
                                                                                                                               //(Yukarida yaptigimiz gibi son kismi tekrarliyoruz baslangica dönmemesi icin)
                        {
                            if (Harita[Dusman[i].DusmanX, Dusman[i].DusmanY + 1, 1] != 2) //Dusmanin asagıya inebilirligini kontrol eder.
                            {
                                Dusman[i].DusmanY++; //asagıya tasır 
                            }
                        }
                    }
                }
                else
                {
                    Dusman[i].Hareket = true; //Dusmanlarin ortusmesini durdurur 
                }
            }
        }

        public void OyuncuHareket()
        {
            //Oyuncuyu tasirken tasima yonunu belirler
            switch (Yon_bilgisi)
            {
                case 1: //Yukari
                    if (playerY - 1 > -1) //Duvar icine girip girmedigini kontrol eder
                    {
                        if (Harita[playerX, playerY - 1, 1] != 2) //Hareket edilecek kismin bos olup olmadigini kontrol eder ( == 2 nin anlami duvar, != 2 anlami yem veya bosluk)
                        {
                            playerY--; //Oyuncuyu dik dogrultuda hareket ettirir.
                        }
                        else
                        {
                            Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                            Agiz = 0;
                        }
                    }
                    else
                    {
                        Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                        Agiz = 0;
                    }
                    break;
                case 2: //Asagi
                    if (playerY + 1 < 20) //Duvar icine girip girmedigini kontrol eder
                    {
                        if (Harita[playerX, playerY + 1, 1] != 2) //Hareket edilecek kismin bos olup olmadigini kontrol eder ( == 2 nin anlami duvar, != 2 anlami yem veya bosluk)
                        {
                            playerY++; //Oyuncuyu dik dogrultuda hareket ettirir.
                        }
                        else
                        {
                            Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                            Agiz = 0;
                        }
                    }
                    else
                    {
                        Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                        Agiz = 0;
                    }
                    break;
                case 3: //Left 
                    if (playerX - 1 > -1) //Duvar icine girip girmedigini kontrol eder
                    {
                        if (Harita[playerX - 1, playerY, 1] != 2) //Hareket edilecek kismin bos olup olmadigini kontrol eder ( == 2 nin anlami duvar, != 2 anlami yem veya bosluk)
                        {
                            playerX--; //Oyuncuyu yatay dogrultuda hareket ettirir.
                        }
                        else
                        {
                            Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                            Agiz = 0;
                        }
                    }
                    else
                    {
                        Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                        Agiz = 0;
                    }
                    break;
                case 4: //Right 
                    if (playerX + 1 < 20) //Duvar icine girip girmedigini kontrol eder
                    {
                        if (Harita[playerX + 1, playerY, 1] != 2) //Hareket edilecek kismin bos olup olmadigini kontrol eder ( == 2 nin anlami duvar, != 2 anlami yem veya bosluk)
                        {
                            playerX++; //Oyuncuyu yatay dogrultuda hareket ettirir.
                        }
                        else
                        {
                            Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                            Agiz = 0;
                        }
                    }
                    else
                    {
                        Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                        Agiz = 0;
                    }
                    break;
                default: //Hareket yok bu yüzden durdurur.
                    break;
            }
        }

        /// bu methodla dusmanlarin ortusmesini engellemeye calisiyoruz
        public void DusmanlarinUstUsteGelmeDurumunuKontrolEt()
        {
            bool Ortusme = false; //Dusmanlar arasinda ortusme olup olmadigini kontrol eder. 

            for (int i = 0; i < Dusman.Count; i++) //Oyun icindeki herbir Dusman icin
            {
                for (int j = 0; j < Dusman.Count; j++)
                {
                    for(int k = 0; k < Dusman.Count; k++)
                    {
                        if (i != j && j != k && k != i)
                        {
                            if (Dusman[i].DusmanX == Dusman[j].DusmanX && Dusman[i].DusmanY == Dusman[j].DusmanY && Dusman[i].DusmanX != (Harita_buyuklugu / 2) - 1 && Ortusme == false)
                            {
                                Dusman[i].Hareket = false; //Eger onları engelleyen birsey varsa bir sonraki hareketlerini saglamak icin degeri false yapiyoruz
                                Ortusme = true; //Bu method bize zaten ortusmeyi buldugunu soylemekte
                            }
                            if (Dusman[j].DusmanX == Dusman[k].DusmanX && Dusman[j].DusmanY == Dusman[k].DusmanY && Dusman[j].DusmanX != (Harita_buyuklugu / 2) - 1 && Ortusme == false)
                            {
                                Dusman[j].Hareket = false; //Eger onları engelleyen birsey varsa bir sonraki hareketlerini saglamak icin degeri false yapiyoruz
                                Ortusme = true; //Bu method bize zaten ortusmeyi buldugunu soylemekte
                            }
                            if (Dusman[k].DusmanX == Dusman[i].DusmanX && Dusman[k].DusmanY == Dusman[i].DusmanY && Dusman[k].DusmanX != (Harita_buyuklugu / 2) - 1 && Ortusme == false)
                            {
                                Dusman[k].Hareket = false; //Eger onları engelleyen birsey varsa bir sonraki hareketlerini saglamak icin degeri false yapiyoruz
                                Ortusme = true; //Bu method bize zaten ortusmeyi buldugunu soylemekte
                            }
                        }
                    }
                }
            }
        }

        /// bu kod yapiyi olusturur ve animasyonu yonetir
        public void OyunuOlustur()
        {
            //en son olusturulan yapiyi silerek yeni bir bir yapi olusturur
            BackBuffer.Clear(Color.Black);

            //Dongu sayesinde butun karelere ulasılır
            for (int x = 0; x < Harita_buyuklugu; x++) //X kordinatlari
            {
                for (int y = 0; y < Harita_buyuklugu; y++) //Y kordinatlari
                {
                    rctDestination = new System.Drawing.Rectangle(x * KareBuyuklugu, y * KareBuyuklugu, KareBuyuklugu, KareBuyuklugu); //Cizilecek Yeni hedefi belirler

                    if (Harita[x, y, 1] == 1) //Mevcut karede yem varmi kontrol eder
                    {
                        BackBuffer.DrawImage(Yem, rctDestination); //Hedef kareye yem yerlestirir.
                    }

                    if (Harita[x, y, 1] == 2) //Mevcut karede duvar varmi kontrol eder.
                    {
                        BackBuffer.FillRectangle(Brushes.Blue, rctDestination); //Hedef kareye duvar yerlestirir. 
                    }
                }
            }

            //Karakteri konumlandirma
            rctDestination = new Rectangle(playerX * KareBuyuklugu, playerY * KareBuyuklugu, KareBuyuklugu, KareBuyuklugu);
            //Karakterin rengini degistirir. 
            BackBuffer.FillEllipse(Brushes.Yellow, rctDestination);

            //Karakterin agiz animasyonu 
            if (Agiz == 1)
            {
                switch (Yon_bilgisi) //hangi yone agiz cizilecegini belirlemek icin switch-case yapisi kulanıyoruz.
                {
                    case 1: //yukari
                        //Karakterin gidis yonune uygun olarak siyah bir ucken cizer 
                        BackBuffer.FillPolygon(Brushes.Black, new Point[] {
                        new Point((playerX * KareBuyuklugu) + (KareBuyuklugu / 2), (playerY * KareBuyuklugu) + (KareBuyuklugu / 2)),
                        new Point((playerX * KareBuyuklugu), (playerY* KareBuyuklugu)),
                        new Point((playerX * KareBuyuklugu) + KareBuyuklugu, (playerY * KareBuyuklugu))
                        });
                        break;
                    case 2: //asagi
                        //Karakterin gidis yonune uygun olarak siyah bir ucken cizer 
                        BackBuffer.FillPolygon(Brushes.Black, new Point[] {
                        new Point((playerX * KareBuyuklugu) + (KareBuyuklugu / 2), (playerY * KareBuyuklugu) + (KareBuyuklugu / 2)),
                        new Point((playerX * KareBuyuklugu), (playerY* KareBuyuklugu) + KareBuyuklugu),
                        new Point((playerX * KareBuyuklugu) + KareBuyuklugu, (playerY * KareBuyuklugu) + KareBuyuklugu)
                        });
                        break;
                    case 3: //sol
                        //Karakterin gidis yonune uygun olarak siyah bir ucken cizer 
                        BackBuffer.FillPolygon(Brushes.Black, new Point[] {
                        new Point((playerX * KareBuyuklugu) + (KareBuyuklugu / 2), (playerY * KareBuyuklugu) + (KareBuyuklugu / 2)),
                        new Point((playerX * KareBuyuklugu), (playerY * KareBuyuklugu)),
                        new Point((playerX * KareBuyuklugu), (playerY * KareBuyuklugu) + KareBuyuklugu)
                        });
                        break;
                    case 4: //sag
                        //Karakterin gidis yonune uygun olarak siyah bir ucken cizer 
                        BackBuffer.FillPolygon(Brushes.Black, new Point[] {
                        new Point((playerX * KareBuyuklugu) + (KareBuyuklugu / 2), (playerY * KareBuyuklugu) + (KareBuyuklugu / 2)),
                        new Point((playerX * KareBuyuklugu) + KareBuyuklugu, (playerY * KareBuyuklugu)),
                        new Point((playerX * KareBuyuklugu) + KareBuyuklugu, (playerY * KareBuyuklugu) + KareBuyuklugu)
                        });
                        break;
                    default: //Hareket etmiyorken
                        //Hicbirsey yapmiyoruz
                        break;
                }
            }


            foreach (Dusman d in Dusman)
            {
                BackBuffer.DrawImage(d.img, d.DusmanX * KareBuyuklugu, d.DusmanY * KareBuyuklugu, KareBuyuklugu, KareBuyuklugu);
                //Butun Dusmanlari olusturur.
            }

            //Can ikonlarını olusturur.
            for (int i = 0; i < Can; i++)
            {
                rctDestination = new Rectangle(KareBuyuklugu + (KareBuyuklugu * i), ((Harita_buyuklugu - 1) * KareBuyuklugu) - 10, KareBuyuklugu, KareBuyuklugu); //Can ikonlarının bulunacagi alani olusturur.
                BackBuffer.FillRectangle(Brushes.Black, rctDestination); //BackColor ile uyusması icin olusturulan alanın BackColorunuda siyah yapıyoruz. 
                BackBuffer.FillEllipse(Brushes.Yellow, rctDestination); //pacmanin gövdesini sari yapar

                //agız kısmını sağ tarafa çeker
                BackBuffer.FillPolygon(Brushes.Black, new Point[] {
                    //merkez noktasi 
                    new Point((KareBuyuklugu + (KareBuyuklugu * i)) + (KareBuyuklugu / 2), ((Harita_buyuklugu - 1) * KareBuyuklugu) + (KareBuyuklugu / 2) - 10),
                    new Point((KareBuyuklugu + (KareBuyuklugu * i)) + KareBuyuklugu, ((Harita_buyuklugu - 1) * KareBuyuklugu) - 10),
                    new Point((KareBuyuklugu + (KareBuyuklugu * i)) + KareBuyuklugu, ((Harita_buyuklugu - 1) * KareBuyuklugu) + KareBuyuklugu  - 10)
                });
            }

            //Ekranin sol üst kısmına puani ve yon bilgisini gosterebilecegimiz bir alan olusturuyoruz
            BackBuffer.DrawString("Puan: " + Puan + Environment.NewLine + "Yon Bilgisi: " + Yon_bilgisi, DefaultFont, Brushes.Lime, new Point(5, 5));

            //Cerceveyi oyun ekranında olusturur. 
            Buffer.DrawImage(Ekran, 0, 0);
        }



        /// Harita dosyasini bir yere kaydetmemizi saglar
        /// <param name="path">Haritanın kaydedilecigi konum to</param>
        public void OyunuKaydet(string path)
        {
            MemoryStream ms = new MemoryStream(); //Bilgileri saklamak icin bellek akisini saglar
            BinaryWriter bw = new BinaryWriter(ms); //binary formatındaki dosyalar üzerinde islem yapmamizi saglar ( okuma ve yazma islemleri )

            for (int x = 0; x < Harita.GetLength(0); x++) //Butun X Degiskenlerini kaydeder
            {
                for (int y = 0; y < Harita.GetLength(1); y++) //Butun Y Degiskenlerini kaydeder
                {
                    for (int z = 0; z < Harita.GetLength(2); z++) //Butun Degiskenlerini kaydeder.
                    {
                        bw.Write(Harita[x, y, z]); //Harita bilgilerini bw degiskenine atar. 
                    }
                }
            }

            //Bilgileri bir dosyaya yazmamizi saglar.
            File.WriteAllBytes(path, ms.ToArray());

            //gecici saklama alanini temizleme
            bw.Dispose();
            ms.Dispose();

        }

        /// Var olan haritayi bir dosyadan yükler
        /// <param name="path">The path of the map file.</param>
        public void OyunuYukle(string path)
        {
            //Dosya okumak icin bir nesne olusturuyoruz 
            BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(path)));

            for (int x = 0; x < Harita.GetLength(0); x++) //Butun X degiskenlerini Okur 
            {
                for (int y = 0; y < Harita.GetLength(1); y++) //Butun Y degiskenlerini Okur  
                {
                    for (int z = 0; z < Harita.GetLength(2); z++) //Butun Z degiskenlerini Okur 
                    {
                        Harita[x, y, z] = br.ReadInt32(); //Harita degiskenini ayarlar.
                    }
                }
            }

            //Temizleme
            br.Close();
            br.Dispose();
            GC.Collect();   //Garbage Collector
        }
    }

    // Yeni bir sınıf Olusturuyoruz
    /// Bu sinif Dusmanların bilgilerini tutmak icin kullanilacak
    class Dusman
    {
        public int DusmanX; //Dusmanın X kordinati 
        public int DusmanY; //Dusmanın Y kordinati
        public Brush Renk; //Dusmanin Rengi
        public bool Hareket; //Dusmanin hareket ediyor mu etmiyor mu 
        public Bitmap img; //Dusman resimlerini tutacak
    }
}