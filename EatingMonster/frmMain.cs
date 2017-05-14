using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


//Gerekli Kütüphaneler 
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;
using System.IO;

namespace EatingMonster
{
    public partial class frmMain : Form
    {
        
        //Dusman Degiskenleri
        List<Dusman> Dusman; //butun dusmanlari tutar.
        int DusmanSirasi = 0; //Dusmanlarin ayri zamanlarda baslamasını saglayacak.

        //Harita Degiskenleri
        string Harita_dosya = Application.StartupPath + "\\Map.map"; //Harita dosyasinin konumu
        int Harita_buyuklugu = 20; //Haritanın buyuklugunu tutacak degisken
        int[, ,] Harita; //Haritayı 3 boyutlu Array formatinda olusturuyoruz (x, y, z)
        int KareBuyuklugu = 32; //Duvarlarin buyuklugu 

        //Oyuncu Degiskenleri 
        int Puan = 0; //Puani tutacak degisken
        int playerX = 0; //Oyuncunun X kordinatini tutacak degisken
        int playerY = 0; //Oyuncunun Y kordinatini tutacak degisken 
        int Yon_bilgisi = 0; //Oyuncunun Hareket yonu 
        int Agiz = 0; //Karakterin animasyon hareketini tutacak degisken
        int Can = 3; //Karakterin kac cani oldugunu tutacak degisken 

        //Grafik Degiskenleri
        Graphics BackBuffer; //double buffer (draws the frame)
        Graphics Buffer; //Ekranda cerceveyi olusturur
        Bitmap Ekran; //Oyunun olusacagi cerceve boyutlarini tutacak 
        Rectangle rctDestination; //Resim ve Kareleri cizerken kulanacagiz. 
        Bitmap Yem; 
        Bitmap Varil1;
        Bitmap Beer1;
        Bitmap hamburger1;
        Bitmap Mantar1;

     



        public frmMain()
        {
            InitializeComponent(); //Formu ve tum degiskenleri hazirlar



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
            Dusman d1 = new Dusman();
            d1.Renk = Brushes.Red;
            d1.img = Properties.Resources.red;
            Dusman.Add(d1);

            //Mavi Dusman
            Dusman d2 = new Dusman();
            d2.Renk = Brushes.Blue;
            d2.img = Properties.Resources.blue;
            Dusman.Add(d2);

            //Pembe Dusman
            Dusman d3 = new Dusman();
            d3.Renk = Brushes.Pink;
            d3.img = Properties.Resources.pink;
            Dusman.Add(d3);
        

            /* Dusmanların varolan konumlarını ayarlar (default konumlarını) */ 
            for (int i = 0; i < Dusman.Count; i++) //Bu dongu sayesinde butun duvarlardan kurtulacak
            {
                Dusman[i].DusmanX = (Harita_buyuklugu / 2) - 1; //Ekranin ortasinda bulunan dusmanin X Kordinati degistirir
                Dusman[i].DusmanY = (Harita_buyuklugu / 2) - 1; //Ekranin ortasinda bulunan dusmanin Y Kordinati degistirir
                Dusman[i].Hareket = true; //Dusmanlarin ust uste gelmesini engeller
                Dusman[i].img.MakeTransparent(Color.Black); //Butun dusmanlarin arkaplan renginin siyah kalmasini saglar 
            }
            

            //Yem resimlerini degistirmek , ve siyahi seffaf yapmak icin (Ortusme icin)
            Yem = Properties.Resources.food;
            Yem.MakeTransparent(Color.Black);
            Varil1 = Properties.Resources.Varil;
            hamburger1 = Properties.Resources.hamburger;
            Beer1 = Properties.Resources.Beer;
            Mantar1 = Properties.Resources.Mantar;

             
            playerX = 9;
            playerY = Harita_buyuklugu - 5;// (HaritaBoyutu -> 20) - 5 = 15 

            //Formun ekrani ortalamasini Saglayan kod
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
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

       
        /// Oyun penceresi gosterilmeden bu method yuklenir.
     
        private void Form1_Load(object sender, EventArgs e)
        {
        
            this.Show(); //Formu gosterir. 
            this.Focus(); //Forma odaklanir.

            /* Fare ve klavyeye el ile özellikler ekleme  */
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(Form1_KeyDown); //Hareket icin 
            this.MouseClick += new MouseEventHandler(Form1_MouseClick); //Duvar duzenlemesi icin 
            this.MouseMove += new MouseEventHandler(Form1_MouseMove); //Duvar duzenlemesi icin 
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing); //Sonsuz donguleri sonlandirmak icin 

            //Formun boyutlarının haritaya tam olarak denk gelmesi için gereken hesaplamalar
            //birimlerin piksel boyutlari * birim sayisi
            this.Height = 32 * 22;
            this.Width = 32 * 21;

            //Oyunu baslatan komutu cagırır. 
            OyunuBaslat();
        }


        /// Form kapandiginda gerceklesmesi gerekenler
        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill(); 
            //Uygulamanin calisma sürecini oldurur
            //bunun amacı uygulamadaki sonsuz dongulerdir 
            //uygulama calismaya devam ettikce sonsuz döngülerde devam edicektir, 
            //Bunu yapmazsak, surec devam edicek dongulerimiz bitmeyecektir.
        }

       

        /// Bu methodta fare formun üzerine geldiginde gerceklesicek olaylar mevcuttur 
        void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //Sol tık yapildigında
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //fare duvarlari bulur 
                int mouseX = 0;
                int mouseY = 0;

                mouseX = e.X / KareBuyuklugu; //Duvarın X kordinatini alır. 
                mouseY = e.Y / KareBuyuklugu; //Duvarın Y kordinatini alır.

                Harita[mouseX, mouseY, 1] = 2;
                
                //Harita üzerinde isaretlenen yeri ekler
            }
            else if(e.Button == System.Windows.Forms.MouseButtons.Right) //sag tıklandiginda
            {
                //fare duvarlari bulur 
                int mouseX = 0;
                int mouseY = 0;

                mouseX = e.X / KareBuyuklugu; //Duvarın X kordinatini alır.  
                mouseY = e.Y / KareBuyuklugu; //Duvarın Y kordinatini alır. 

                Harita[mouseX, mouseY, 1] = 0; //Harita üzerinde isaretlenen yeri siler
                Harita[playerX, playerY, 1] = 0;
            }
        }

        
        
        void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            //Sol tık yapildigında
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //fare duvarlari isaretler
                int mouseX = 0;
                int mouseY = 0;

                mouseX = e.X / KareBuyuklugu; //Duvarın X kordinatini alır.
                mouseY = e.Y / KareBuyuklugu; //Duvarın Y kordinatini alır.

                Harita[mouseX, mouseY, 1] = 2; //Harita üzerinde isaretlenen yeri ekler
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right) //sag tıklandiginda
            {
                //fare duvarlari bulur 
                int mouseX = 0;
                int mouseY = 0;

                mouseX = e.X / KareBuyuklugu; //Duvarın X kordinatini alır.
                mouseY = e.Y / KareBuyuklugu; //Duvarın Y kordinatini alır.

                Harita[mouseX, mouseY, 1] = 0; //arita üzerinde isaretlenen yeri siler
                Harita[playerX, playerY, 1] = 0;
            }
        }
        
       
        /// bu method bir tusa basildiginda gerceklesicek
        void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
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
                    if (Harita[playerX - 1, playerY, 1] != 2) //Duvar icine girip girmedigini kontrol eder
                    {
                        Yon_bilgisi = 0; //Karakterin tasinacagi yeni konumun yonunu belirler
                    }
                    break;
                case Keys.F1: //Harita dosyasını kaydeder. ( calismiyor. ) 
                    OyunuKaydet(Harita_dosya);
                    break;
                case Keys.F2: //Harita dosyasını yükler.
                    OyunuYukle(Harita_dosya);
                    break;
            }
        }

    
        /// Bu metod tüm grafik değişkenlerini kurar ve ana oyun döngüsüne başlar
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
            //Dusmanlar Serbest birakildiginda ust uste gelen dusmanları ayırmak icin

            /*
             * Dusmanlari ayirmak icin Thread kulanicagiz
             * döngü icerisinde thread methodlari aynı anda calisacak olsada 
             * farkli zamanlarda baslamasi gerekicek bunun icin thread.sleep methodunu kulanacagız
            */
            new Thread(() =>
                {
                    while (DusmanSirasi != 0) //Tum dusmanlari birakana dek devam edicek
                    {
                        DusmanSirasi--; //Dusmani serbest birakir
                        Thread.Sleep(10000); //Bir diger dusmani bırakmak icin 1000mS=10sn bekler
                    }
                }).Start(); //Yeni bir thread baslatir.

            /* 
             * Bu thread butun dusmanlari ayni anda hareket ettirecek
             * bu kod her 175ms icinde tekrarlanicak yani 174ms boyunca uyku halinde kalicak
             * (Buda karakterden 25ms daha sonra demek),
             * if this code was ran in the main game draw loop it would freeze up the code as well 
             */ 
            new Thread(() =>
                {
                    while (true) //Dusmanlari sonsuza dek hareket ettiricek
                    {
                        for (int i = 0; i < Dusman.Count - DusmanSirasi; i++) 
                            //teker teker dusmanlar dongu icine giricek
                        
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
                        Thread.Sleep(175); //Tekrar taşımadan önce Dusmanlari 175 ms bekletir.
                        DusmanlarınUstUsteGelmeDurumunuKontrolEt(); //Dusmanlarin ortusmesini durdurur 
                    }
                }).Start(); //Thread lari baslatir.

            /*
             * Oyuncunun Hareketleri + Animasyon Ayarları,
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
                                }
                                else
                                {
                                    Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                                }
                                break;
                            case 2: //Asagi
                                if (playerY + 1 < 20) //Duvar icine girip girmedigini kontrol eder
                                {
                                    if (Harita[playerX, playerY + 1, 1] != 2) //Hareket edilecek kismin bos olup olmadigini kontrol eder ( == 2 nin anlami duvar, != 2 anlami yem veya bosluk)
                                    {
                                        playerY++; //Oyuncuyu dik dogrultuda hareket ettirir.
                                    }
                                }
                                else
                                {
                                    Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                                }
                                break;
                            case 3: //Left 
                                if (playerX - 1 > -1) //Duvar icine girip girmedigini kontrol eder
                                {
                                    if (Harita[playerX - 1, playerY, 1] != 2) //Hareket edilecek kismin bos olup olmadigini kontrol eder ( == 2 nin anlami duvar, != 2 anlami yem veya bosluk)
                                    {
                                        playerX--; //Oyuncuyu yatay dogrultuda hareket ettirir.
                                    }
                                }
                                else
                                {
                                    Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                                }
                                break;
                            case 4: //Right 
                                if (playerX + 1 < 20) //Duvar icine girip girmedigini kontrol eder
                                {
                                    if (Harita[playerX + 1, playerY, 1] != 2) //Hareket edilecek kismin bos olup olmadigini kontrol eder ( == 2 nin anlami duvar, != 2 anlami yem veya bosluk)
                                    {
                                        playerX++; //Oyuncuyu yatay dogrultuda hareket ettirir.
                                    }
                                }
                                else
                                {
                                    Yon_bilgisi = 0; //Hareket edemedigi noktada Karakteri durdurur.
                                }
                                break;
                            default: //Hareket yok bu yüzden durdurur.
                                break;
                        }

                        
                        


                        if (Harita[playerX, playerY, 1] == 1) //Mevcut zemin eger 1re esit ise yiyebilecegi yem var
                         {

                            Puan += 10; //Yem yediginde puan degiskenini arttirir.
                            Harita[playerX, playerY, 1] = 0; //Yemi harita uzerinden kaldirir.
                         }


                        // Diger yemlerin verdikleri ozellikler
               
                        while (Harita[playerX, playerY, 1] == Harita[18, 16, 1])
                        {
                            Thread.Sleep(20);//karakteri yavaslatir.
                            break;
                        }

                        while (Harita[playerX, playerY, 1] == Harita[12, 6, 1])
                        {
                            Thread.Sleep(30);//karakteri yavaslatir.
                            break;
                        }

                        while (Harita[playerX, playerY, 1] == Harita[4, 11, 1])
                        {
                            Thread.Sleep(40);//karakteri yavaslatir.
                            break;
                        }

                        while (Harita[playerX, playerY, 1] == Harita[10, 1, 1])
                        {
                            Thread.Sleep(50);//karakteri yavaslatir.
                            break;
                        }

                        
                        

                        //Oyuncunun olu olup olmadigini kontrol eder
                        for (int i = 0; i < Dusman.Count; i++) //Bu dongu butun dusmanlar icin gecerli olacaktir.
                        {
                            if (Dusman[i].DusmanX == playerX && Dusman[i].DusmanY == playerY) //Dusman ile karakterin kesisme durumunu kontrol eder. 
                            {
                                Can--; //Bir adet can silinir. 
                               
                 
                                Thread.Sleep(2000); //Karakter oldukten sonra 2000ms bekletir.

                                if (Can == 0) //Oyuncunun canlarinin tukenip tukenmedigini kontrol eder.
                                {

                                    // frmMain.ActiveForm.Hide(); // ( Not : Hata veriyor , oyun ekrani kapanmiyor.)

                                    LoseForm frm = new LoseForm();
                                    frm.ShowDialog();

                                    System.Diagnostics.Process.GetCurrentProcess().Kill(); //Programi ve butun sonsuz donguleri sonlandirir.
   
                                }

                                //Oyuncu butun canlarini kaybetti, Bu yuzden butun konumlar sıfırlanıcak
                                KonumSifirlama();

                                
                            }
                        }

                        //Bu donguyu 150ms de bir calistiri(150ms olmasının nedeni 25ms dusmanlardan avantajli olmasını saglayacak) 
                        Thread.Sleep(150);
                    }
                }).Start(); 

            //Gercek Oyun dongusu
            while (true)
            {
                //Bir windows form çalıştırılığında, döngü içerisinde gerçekleştirilmek istenen olaylar bir kuyruğa alınır
                //ve bu kuyrukta bekletilir. Olayların kuyrukta bekletiliyor olması,  uygulamanın cevap vermemesine yol acar
                //Buda uygulamada hatalara sebep olabilir bunu onlemek icin Application.DoEvents() kullanılır.
                Application.DoEvents();

                //Oyuncu butun yemleri topladiginda oyunu bitirir.
                if (Puan >= 1770)
                {

                    frmMain.ActiveForm.Hide(); 

                    WinForm frm = new WinForm();
                    frm.ShowDialog();

                    //butun islemleri bitirir.
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }

                //DrawGame methodunu cagiralim
                OyunuOlustur();
                
            }
        }
        
        
        /// bu methodla dusmanlarin ortusmesini engellemeye calisiyoruz
        public void DusmanlarınUstUsteGelmeDurumunuKontrolEt()
        {
            bool Ortusme = false; //Dusmanlar arasinda ortusme olup olmadigini kontrol eder. 

            for (int i = 0; i < Dusman.Count; i++) //Oyun icindeki herbir Dusman icin
            {
                for (int j = 0; j < Dusman.Count; j++)
                {
                    if (j != i)
                    {
                        if (Dusman[j].DusmanX == Dusman[i].DusmanX && Dusman[j].DusmanY == Dusman[i].DusmanY && Dusman[i].DusmanX != (Harita_buyuklugu / 2) - 1 && Ortusme == false) //Dusmanlarin ortusmesini kontrol eder
                        {
                            Dusman[j].Hareket = false; //Eger onları engelleyen birsey varsa bir sonraki hareketlerini saglamak icin degeri false yapiyoruz
                            Ortusme = true; //Bu method bize zaten ortusmeyi buldugunu soylemekte
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



                    if (Harita[x, y, 1] == 1)//Mevcut karede Duvar varmi kontrol eder
                    {

                        BackBuffer.DrawImage(Varil1, 320, 32);          // varil resmini konumunu yerlestirir.
                        BackBuffer.DrawImage(Mantar1, 384, 192);        // Mantar resmini konumunu yerlestirir.
                        BackBuffer.DrawImage(Beer1, 128, 352);          // bira resmini konumunu yerlestirir.
                        BackBuffer.DrawImage(hamburger1, 576, 512);     // hamburger resmini konumunu yerlestirir.
                }



                    if (Harita[x, y, 1] == 1) //Mevcut karede Duvar varmi kontrol eder
                    {
                        
                        BackBuffer.DrawImage(Yem, rctDestination); //Hedef kareye yem yerlestirir.
                    }

                    if (Harita[x, y, 1] == 2) //Mevcut karede yem varmi kontrol eder.
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
                rctDestination = new Rectangle(10 + (KareBuyuklugu * i), ((Harita_buyuklugu - 1) * KareBuyuklugu) + 10, KareBuyuklugu, KareBuyuklugu); //Can ikonlarının bulunacagi alani olusturur.
                BackBuffer.FillRectangle(Brushes.Black, rctDestination); //BackColor ile uyusması icin olusturulan alanın BackColorunuda siyah yapıyoruz. 
                BackBuffer.FillEllipse(Brushes.Yellow, rctDestination); //pacmanin gövdesini sari yapar 

                //agız kısmını sağ tarafa çeker
                BackBuffer.FillPolygon(Brushes.Black, new Point[] {
                    //merkez noktasi 
                    new Point(((10 + (KareBuyuklugu * i))) + (KareBuyuklugu / 2), ((Harita_buyuklugu - 1) * KareBuyuklugu) + (KareBuyuklugu / 2) + 10),
                    //Dis noktalar
                    new Point(((10 + (KareBuyuklugu * i))) + KareBuyuklugu, ((Harita_buyuklugu - 1) * KareBuyuklugu) + 10),
                    new Point(((10 + (KareBuyuklugu * i))) + KareBuyuklugu, ((Harita_buyuklugu - 1) * KareBuyuklugu) + KareBuyuklugu + 10)
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
            GC.Collect();
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
