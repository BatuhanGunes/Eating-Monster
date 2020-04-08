**Dil :** [English](https://github.com/BatuhanGunes/Pacman) / Turkish

# Pacman

<img align="right" width="300" height="300" src="https://github.com/BatuhanGunes/Pacman/blob/master/Screenshot/Game3.gif">

Bu proje klasik Pacman oyununun bir versiyonu olarak programlanmıştır. Oyun, pacman adlı karakterimizi yönlendirerek, harita üzerinde önceden belirlenmiş konumlarda bulunan tüm yem nesnelerinin üzerinden geçmesiyle tamamlanır. Üzerinden geçilen yemler yok olur ve 10 puan eklenir. Bu aşamada Pacman'in 3 farklı düşmandan kaçması gerekmektedir. Karakterin 3 canı vardır. Eğer karakter düşmanlardan herhangi biri ile temas ederse bir adet canını kaybeder. Bütün canların bitmesi sonucu oyun sonlandırılır ve bölüm başarısız kabul edilir. Aynı zamanda karakter ve düşmanlar haritada bulunan bir başka nesne olan duvarlardan geçemezler. Düşmanlar karaktere ulaşmak için onun bulunduğu konuma hücum ederler. Karakter ise kullanıcı tarafından yönetilir. Karakterin yönllendirilmesinde kullanılan fonksiyonları şu şekilde açıklayabiliriz;

* W -> Yukarı hareketi sağlar.
* A -> Sola hareketi sağlar.
* S -> Aşağıya hareketi sağlar
* D -> Sağa hareketi sağlar
* Q -> Durmayı sağlar.

<img align="right" width="300" height="300" src="https://github.com/BatuhanGunes/Pacman/blob/master/Screenshot/MapEditor.gif">

Oyunun bir başka fonksiyonuda bölümleri kendinizin düzenleyebilmenizdir. Düzenleme işlemi bittikten sonra yeni haritayı kaydedebilir veya farklı bir harita yükleyebilirsiniz. Bu işlem için mouse tuşlarını kullanmak gerekmektedir. Tuşlar tek tıkklama veya tıklayıp sürükleme şeklinde kullanılabilir. Bu tuşlar ve fonksiyonları;

- Sol tuş  -> Bulunan konuma duvar nesnesi ekler.
- Orta tuş -> Bulunan konuma yem nesnesi ekler.
- Sağ tuş  -> Bulunan konumdaki nesneyi kaldırır.
- F1       -> Haritayı dosya yoluna kaydeder.
- F2       -> Haritayı dosyadan yükler.

```
Projenin oluşturulma tarihi : Nisan 2017
```

## Ekran Görüntüleri

<img width="275" height="275" src="https://github.com/BatuhanGunes/Pacman/blob/master/Screenshot/HomePage.jpg"> <img width="275" height="275" src="https://github.com/BatuhanGunes/Pacman/blob/master/Screenshot/GameStart.jpg"> <img width="275" height="275" src="https://github.com/BatuhanGunes/Pacman/blob/master/Screenshot/Game.png">


## Başlangıç

Projeyi çalıştırabilmek için proje dosyalarının bir kopyasını yerel makinenize indirin. Gerekli ortamları edindikten sonra projeyi bu ortamda açarak çalıştırabilir ve çalıştırıldıktan sonra açılan pencere üzerinden uygulamayı kullanabilirsiniz. İkinci kez çalıştırılmak istenildiğinde, projenin bulunduğu konumda ~\EatingMonster\bin\Debug\EatingMonster.exe dosyasını çalıştırmanız yeterli olacaktır.

### Gereklilikler

- Microsoft Visual Studio 

Projeyi çalıştırabilmek için ilk olarak [Microsoft Visual Studio ](https://visualstudio.microsoft.com/) adresinden sisteminize uygun C# IDE olan Microsoft Visual Studio yazılımının herhangi bir sürümünü edinerek yerel makinenize kurmanız gerekmektedir. Daha sonra projemizi IDE ortamına tanıtıp debug işlemini gerçekleştirmeniz yeterli olacaktır. Eğer bir program indirmek istemiyorsanız [Microsoft Visual Studio Online](https://visualstudio.microsoft.com/tr/services/visual-studio-online/) adresinden projeyi çalıştırabilirsiniz.

## Yazarlar

* **Batuhan Güneş**  - [BatuhanGunes](https://github.com/BatuhanGunes)

Ayrıca, bu projeye katılan ve katkıda bulunanlara [contributors](https://github.com/BatuhanGunes/Pacman/graphs/contributors) listesinden ulaşabilirsiniz.

## Lisans

Bu proje Apache lisansı altında lisanslanmıştır - ayrıntılar için [LICENSE.md](https://github.com/BatuhanGunes/Pacman/blob/master/LICENSE) dosyasına bakabilirsiniz.

