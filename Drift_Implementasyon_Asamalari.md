# ARCADE CAR — DRIFT İMPLEMENTASYON AŞAMALARI (test adımları gömülü)

Bu dosya `Arcade_Car_Drift_Implementasyon_Siralamasi.txt` belgesindeki 21 aşamayı,
sonundaki "kısa liste"deki test/kontrol adımlarını her aşamanın içine yerleştirerek
tek akışta toparlar.

Her aşama şu bölümlerden oluşur:
- **Amaç** — bu aşama neyi çözüyor
- **Yapılacak** — kod/ayar değişikliği
- **Test / Kontrol** — nasıl doğrulanır
- **Geçiş kriteri** — bir sonraki aşamaya ne zaman geçilir

## Altın kural
Aynı anda birden fazla fizik parametresini drift'e bağlama. Bir seferde tek parametre:
bağla → test et → oturt → sonrakine geç. Sorun çıktığında sebebin tek olması için.

## Mimari sınırları (hiç ihlal edilmez)
- `CarInput` karar vermez, Rigidbody'ye dokunmaz.
- `CarPhysics` fiziksel veriyi üretir **ve** kuvvetleri uygular.
- `DriftEvaluator` hiçbir `AddForce` / `AddTorque` çağrısı yapmaz.
- `CarBehaviour` Rigidbody'ye dokunmaz; sadece `Current*` değerleri üretir.
- `CarController` detay bilmez: input okumaz, fizik hesaplamaz, karar vermez.
- `CarSuspension` drift sisteminin ilk aşamalarında değişmez.

---

# AŞAMA 0 — Mevcut sistemi sabitle

**Amaç:** Drift eklenmeden önce "normal araba" davranışını referans olarak bilmek.
İleride problem çıkınca "fizik mi bozuldu, evaluator mı yanlış ölçüyor, behaviour mı yanlış
parametre veriyor" ayrımını yapabilmek.

**Yapılacak:** Kod değişmez. Sadece mevcut değerler not edilir:
- `CarConfig`: acceleration, turnSpeed, lateralGrip, lateralForceHeight, forwardDrag, angularDrag
- `CarSuspension`: suspensionLength, springStrength, damperStrength

**Gerçek baseline (Inspector değerleri — CarConfig default'ları değil):**

| Parametre | Değer | Not |
|---|---|---|
| acceleration | 8000 | |
| turnSpeed | **3000** | angularDrag bastırdığı için 1000'den yükseltildi |
| lateralGrip | 8 | drift'te Aşama 6'da düşürülecek |
| lateralForceHeight | 0.5 | |
| forwardDrag | **1** | gaz kesince araba biraz daha kaysın diye 2'den düşürüldü |
| angularDrag | **5 (sabit)** | "sürekli dönmeyi engelleme" hissi beğenildi; drift'te Aşama 11'de `driftAngularControl` ile düşürülecek |

Karar: angularDrag baseline'da değiştirilmez. Dönüş yetersizliği turnSpeed artışıyla çözüldü.

**Test / Kontrol:**
- Arabayı düz sür: stabil mi, titriyor mu?
- Viraja gir/çık: normal toparlıyor mu?
- Bu davranışı (video veya not) referans olarak sakla.

**Geçiş kriteri:** Normal sürüş davranışı belgelenmiş ve tekrarlanabilir durumda.

---

# AŞAMA 1 — CarPhysics'e vehicle state ölçümleri ekle

**Amaç:** Evaluator'ın güvenilir veri kaynağına ihtiyacı var. Rigidbody hesaplarını
evaluator değil, CarPhysics yapmalı.

**Yapılacak:** `CarPhysics` içine:
```
public float ForwardVelocity { get; private set; }
public float LateralVelocity { get; private set; }
public float Speed         { get; private set; }
public float YawVelocity   { get; private set; }

public void UpdateVehicleState()
{
    Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
    ForwardVelocity = localVelocity.z;
    LateralVelocity = localVelocity.x;
    Speed = rb.linearVelocity.magnitude;

    Vector3 localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);
    YawVelocity = localAngularVelocity.y;
}
```
`CarController.FixedUpdate` en başında `carPhysics.UpdateVehicleState()` çağrılır.

**Test / Kontrol:**
- Derleniyor, hata yok.
- Değerler NaN/patlama üretmiyor (geçici bir `Debug.Log` ile bak).

**Geçiş kriteri:** State fonksiyonu her FixedUpdate'te güncelleniyor.

---

# AŞAMA 2 — Debug ekranını genişlet

**Amaç:** Drift eşiklerini tahminle değil, ölçümle belirlemek.

**Yapılacak:** `CarController.OnGUI` içine geçici olarak ekle:
- SteerInput
- ForwardVelocity
- LateralVelocity
- Speed
- YawVelocity

**Test / Kontrol (kısa liste madde 3, 6):** Arabayı sürerken sayıların beklendiği gibi
değiştiğini gözle:
- Düz giderken: SteerInput ≈ 0, LateralVelocity ≈ 0, YawVelocity ≈ 0
- Viraja girerken: SteerInput yükselir, YawVelocity yükselir
- Kayma başlayınca: LateralVelocity belirgin şekilde yükselir

**Geçiş kriteri:** Beş değer de ekranda ve sürüşle tutarlı hareket ediyor.

---

# AŞAMA 3 — DriftEvaluator class'ını ekle

**Amaç:** Intent + vehicle state'i tek yerde yorumlayıp `DriftAmount` üretmek.

**Yapılacak:** Yeni dosya `DriftEvaluator.cs`:
```
public float DriftAmount { get; private set; }   // 0.0 = drift yok, 1.0 = maksimum

public void Evaluate(CarInput input, CarPhysics physics) { ... }
```
İlk versiyonda state yok, sadece sürekli değer. Girdiler: SteerInput, Speed,
LateralVelocity, YawVelocity. Hiçbir AddForce/AddTorque yok.

**Test / Kontrol:** Derleniyor; `Evaluate` çağrılmasa bile referans bağlanabiliyor.

**Geçiş kriteri:** Class var, `DriftAmount` property'si erişilebilir.

---

# AŞAMA 4 — İlk drift formülünü çok basit tut

**Amaç:** Mükemmel matematik değil, veri akışını test etmek.

**Yapılacak:** Üç faktörden skor:
```
steerFactor   = abs(SteerInput)                       // oyuncu ne kadar sert dönmek istiyor
lateralFactor = normalize(abs(LateralVelocity))       // araba gerçekten ne kadar yana kayıyor
speedFactor   = 0..3 m/s -> 0 ; 3..10 -> kademeli ; 10+ -> 1

DriftAmount = steerFactor * lateralFactor * speedFactor
```
Slip angle YOK. Bu nihai formül değil.

**Test / Kontrol:** Formül 0–1 aralığında kalıyor mu (Mathf.Clamp01).

**Geçiş kriteri:** `DriftAmount` hesaplanıyor ama henüz hiçbir yere bağlı değil.

**Uygulandı (`DriftEvaluator.cs`):** Inspector'dan tune edilebilen sabitler:
- `minDriftSpeed = 3`, `fullDriftSpeed = 10` → speedFactor
- `lateralVelocityForFullDrift = 6` → lateralFactor
- Slip angle yok. `DriftAmount = steerFactor * lateralFactor * speedFactor`
- Not: baseline'da grip = 8 olduğu için kayma ~0; `DriftAmount` ancak grip
  düştükten (Aşama 6) veya Play modda grip elle indirilince anlamlı değer üretir.

---

# AŞAMA 5 — DriftEvaluator'ı CarController'a bağla (sadece ölçüm)

**Amaç:** Doğru FixedUpdate sırasını kurmak. Feedback loop: Physics → Measure →
Evaluate → Behaviour → Physics.

**Yapılacak:** `CarController`:
```
carPhysics.UpdateVehicleState();
driftEvaluator.Evaluate(carInput, carPhysics);
carBehaviour.Evaluate(carInput.ThrottleInput, carInput.SteerInput);   // driftAmount HENÜZ verilmiyor
// ApplyMovement / ApplySteering / ApplyForwardDrag / ApplyLateralGrip / ApplyAngularDrag
```
`RequireComponent`'e `DriftEvaluator` eklenebilir.

**Test / Kontrol (kısa liste madde 6, 7):**
- `DriftAmount`'u OnGUI'de göster.
- Behaviour'a HENÜZ bağlama. Sadece izle:
  - Düz sürüşte ≈ 0
  - Sert steer + kayma başlayınca yükseliyor
  - Mouse bırakınca düşüyor
  - Çok yavaşken (Speed < 3) steer verince yükselmiyor

**Geçiş kriteri:** `DriftAmount` sürüş gerçekliğiyle tutarlı tepki veriyor. Araba
davranışı Aşama 0'a göre **hiç değişmemiş** olmalı (henüz behaviour'a dokunulmadı).

---

# AŞAMA 6 — CarBehaviour DriftAmount'ı kullanmaya başlasın (SADECE grip)

**Amaç:** İlk gerçek drift davranışı — tek parametre.

**Yapılacak:**
- `CarBehaviour.Evaluate` imzasına `float driftAmount` eklenir.
- `CarController` artık `carBehaviour.Evaluate(throttle, steer, driftEvaluator.DriftAmount)` çağırır.
- `CarConfig`'e (ya da yeni `DriftConfig`'e) `driftGrip` eklenir (ör. 4).
```
CurrentGrip = Mathf.Lerp(config.lateralGrip, config.driftGrip, driftAmount);
```
Diğer tüm `Current*` değerleri hâlâ config'den birebir. **Sadece grip.**

**Test / Kontrol:** driftAmount 0 → grip 8, driftAmount 0.5 → grip 6, driftAmount 1 → grip 4
(OnGUI'ye `CurrentGrip` ekleyip doğrula).

**Geçiş kriteri:** Grip, `DriftAmount` ile beklendiği gibi lerp'liyor.

---

# AŞAMA 7 — Grip feedback loop'unu test et (KRİTİK)

**Amaç:** "Sert steer → lateral velocity → DriftAmount ↑ → grip ↓ → daha fazla kayma →
DriftAmount ↑↑" döngüsünün kontrolden çıkıp çıkmadığını görmek.

**Yapılacak:** Kod değişmez. Bu bir test aşaması. Başka hiçbir fizik parametresine dokunma.

**Test / Kontrol:**
- `DriftAmount` çok hızlı mı 0 → 1 oluyor?
- Grip çok hızlı mı düşüyor?
- Drift kendini büyütüp araba spin mi atıyor?
- Mouse bırakınca araba toparlıyor mu?

Sorun varsa çözüm: `driftGrip` değerini yumuşat (4 yerine 5–6), formüldeki
`speedFactor`/`lateralFactor` normalize aralığını genişlet. Smoothing bir sonraki aşamada.

**Geçiş kriteri:** Drift başlatılabiliyor, mouse bırakınca araba kendini topluyor,
kontrolsüz spin yok.

---

# AŞAMA 8 — DriftAmount'a smoothing ekle

**Amaç:** Mouse ve fizik değerleri frame/frame zıplıyor; `DriftAmount` titremesin.

**Yapılacak:** `DriftEvaluator` içinde target/current ayrımı:
```
TargetDriftAmount = hesaplanan;
DriftAmount = Mathf.MoveTowards(DriftAmount, TargetDriftAmount, transitionSpeed * Time.fixedDeltaTime);
// veya SmoothDamp
```

**Test / Kontrol:** OnGUI'de `DriftAmount` artık `0.20 → 0.23 → 0.28 → 0.34` gibi
akıyor mu; `0.1 → 0.8 → 0.2 → 0.9` gibi zıplama bitti mi?

**Geçiş kriteri:** Değer geçişleri yumuşak, arcade hissi tutarlı.

---

# AŞAMA 9 — Drift'e giriş ve çıkışı ayır (hysteresis)

**Amaç:** Eşik civarında (`0.39 / 0.41 / 0.39 / 0.42`) sürekli durum değişimini önlemek.

**Yapılacak:** `DriftEvaluator` içinde:
- Drift'e giriş şartı: `DriftAmount > 0.45`
- Drift'ten çıkış şartı: `DriftAmount < 0.25`

İlk versiyonda hâlâ sadece `DriftAmount` sürekli değeri kullanılabilir; state sonraya kalır.

**Test / Kontrol (kısa liste madde 12):** Eşik civarında sabit hızla dönerken davranış
seğirmiyor; drift'e giriş/çıkış net.

**Geçiş kriteri:** Giriş/çıkış stabil, flip-flop yok.

---

# AŞAMA 10 — Steering power'ı drift'e bağla

**Amaç:** İkinci parametre. Grip stabil olduktan **sonra**.

**Yapılacak:** `CarConfig`'e `driftTurnSpeed` (ör. 110). 
```
CurrentSteeringPower = Mathf.Lerp(config.turnSpeed, config.driftTurnSpeed, driftAmount);
```

**Test / Kontrol:** Küçük artışla başla: `100 → 110`. Test et. Sonra `100 → 120`.
Araba drift mi yapıyor yoksa spin mi atıyor? Spin varsa artışı düşür.

**Geçiş kriteri:** Steering artışı drift'i besliyor, kontrolü bozmuyor.

---

# AŞAMA 11 — Angular control'u drift'e bağla

**Amaç:** `ApplyAngularDrag` yaw'ı bastırıyor; drift sırasında fazla güçlüyse drift hemen ölür.

**Yapılacak:** `CarConfig`'e `driftAngularControl` (ör. 2).
```
CurrentAngularControl = Mathf.Lerp(config.angularDrag, config.driftAngularControl, driftAmount);
```

**Test / Kontrol:**
- Normal sürüşte araba dönüşten sonra hızlı toparlanıyor (değer yüksek).
- Drift'te yaw hareketine daha çok izin var (değer düşük), ama araba sonsuz dönmeye başlamıyor.

**Geçiş kriteri:** Drift açısı korunabiliyor, recovery hâlâ mümkün.

---

# AŞAMA 12 — Forward drag'i (gerekirse) drift'e bağla

**Amaç:** Drift'in hız maliyetini ayarlamak. Drift **oluşturmak** için ana araç değildir.

**Yapılacak:** `CarConfig`'e `driftForwardDrag`. Örn. `2 → 1.4` (driftte hız korunur) veya
tersi (driftte hız kaybı artar).
```
CurrentForwardDrag = Mathf.Lerp(config.forwardDrag, config.driftForwardDrag, driftAmount);
```

**Test / Kontrol:** Drift çıkışında hız hissi oyuna uygun mu? Sadece bu değeri oynat.

**Geçiş kriteri:** Drift'in hız ekonomisi istenildiği gibi.

---

# AŞAMA 13 — Lateral force height'ı en son tune et

**Amaç:** Drift sırasında gövde/roll davranışı. Süspansiyonla çok bağlantılı, o yüzden en sona.

**Yapılacak:** Önce `DriftAmount`, Grip, Steering, AngularControl stabil olmalı. Sonra:
```
CurrentLateralForceHeight = Mathf.Lerp(config.lateralForceHeight, config.driftForceHeight, driftAmount);
```

**Test / Kontrol:** Drift'te gövde yatışı abartılı/ters değil; süspansiyon zıplamıyor.

**Geçiş kriteri:** Görsel gövde davranışı drift hissini destekliyor.

---

# AŞAMA 14 — Slip angle ölçümünü ekle

**Amaç:** Daha kaliteli drift ölçümü için arabanın baktığı yön ile hareket yönü farkı.

**Yapılacak:** `CarPhysics`:
```
public float SlipAngle { get; private set; }
// UpdateVehicleState içinde:
SlipAngle = Mathf.Atan2(LateralVelocity, Mathf.Abs(ForwardVelocity)) * Mathf.Rad2Deg;
```
Debug ekranına ekle.

**Test / Kontrol:** Sürerken kabaca:
- 0–3° düz sürüş, 5–10° hafif kayma, 10–20° drift, 20°+ güçlü drift / spin riski.
- Kesin eşikler testle bulunur.

**Geçiş kriteri:** SlipAngle ölçülüyor ve sürüşle tutarlı.

---

# AŞAMA 15 — DriftEvaluator formülünü SlipAngle ile geliştir

**Amaç:** Basit çarpım formülünü daha gerçekçi bir slip ölçümüyle güçlendirmek.

**Yapılacak:** `DriftEvaluator` formülüne SlipAngle'ı en önemli girdilerden biri olarak ekle
(örn. `actualSlip = normalize(SlipAngle, 5°, 20°)`).

**Test / Kontrol:** Aşama 5'teki gözlem testlerini tekrarla — `DriftAmount` artık daha
"gerçek" kaymayı yansıtıyor mu?

**Geçiş kriteri:** DriftAmount, düşük hızda tam steer gibi sahte durumlarda yükselmiyor;
gerçek kaymada yükseliyor.

---

# AŞAMA 16 — Intent ve actual drift'i ayır

**Amaç:** "Oyuncu drift istiyor" ≠ "araba drift ediyor".

**Yapılacak:** `DriftEvaluator` iki ayrı skor:
```
playerIntent = f(SteerInput, mouse drag miktarı, ileride drag hızı/yönü)
actualSlip   = f(LateralVelocity, SlipAngle, YawVelocity, Speed)

DriftAmount = playerIntent * actualSlip * speedFactor
```

**Test / Kontrol:** `SteerInput = 1, Speed = 1 m/s` → `DriftAmount` düşük kalıyor mu?

**Geçiş kriteri:** İki skor ayrı ayrı doğru; çarpımları mantıklı.

---

# AŞAMA 17 — Drift recovery davranışını tasarla

**Amaç:** Mouse bırakılınca (`SteerInput → 0`) ama LateralVelocity/YawVelocity hâlâ
yüksekken araba "snap" yapıp bir anda düzelmesin.

**Yapılacak:** Recovery sırasında (DriftAmount düşerken ama araç hâlâ yana hareketliyken):
- grip kademeli artar
- angular control kademeli artar
- steering normalleşir

Smoothing (Aşama 8) çoğu işi yapar; gerekirse recovery'e özel daha yavaş transition hızı.

**Test / Kontrol:** Yüksek hızlı drift'ten mouse'u aniden bırak — araba kademeli mi
toparlıyor, yoksa anında mı hizalanıyor?

**Geçiş kriteri:** Drift çıkışı yumuşak ve kontrollü.

---

# AŞAMA 18 — Spin risk / limiter (stability assist) ekle

**Amaç:** Oyuncu sürekli spin atıyorsa drift eğlenceli olmaz.

**Yapılacak:** Normal drift behaviour'dan **ayrı** bir katman. SlipAngle çok yüksekse:
- steering power azalt
- grip biraz artır
- angular control artır

**Test / Kontrol:** Bilerek aşırı sert drift dene — limiter devreye girip arabayı
kurtarıyor mu, ama normal drift'i bozmuyor mu?

**Geçiş kriteri:** Spin nadir; normal drift limiter'dan etkilenmiyor.

---

# AŞAMA 19 — Mouse input'u geliştir

**Amaç:** Şu an sadece `dragX / maxDragDistance`. Jest bilgisini kullanmak.

**Yapılacak:** `CarInput`'a ekle: drag distance, drag speed, drag direction, pointer
velocity, gesture angle. Hızlı/sert swipe → drift initiation boost; yavaş drag → normal steering.

**Test / Kontrol:** Hızlı swipe drift'i tetikliyor, yavaş drag normal dönüş veriyor mu?
`CarPhysics` / `CarBehaviour` değişmeden çalışıyor mu?

**Geçiş kriteri:** Jest çeşitliliği drift hissini zenginleştiriyor, mimari sınırlar korunmuş.

---

# AŞAMA 20 — Mobil touch input'a geç

**Amaç:** CarInput katmanının ayrık olmasının asıl faydası.

**Yapılacak:** `CarPhysics` ve `CarBehaviour` değişmeden — mobilde touch position / drag
aynı `SteerInput`'u üretir. Sadece `CarInput`'un kaynağı değişir.

**Test / Kontrol:** Mobilde/simülatörde drift sistemi masaüstüyle aynı davranıyor mu?

**Geçiş kriteri:** Drift sistemi yeniden yazılmadan mobilde çalışıyor.

---

## (Opsiyonel) State katmanı — Aşama 16 sonrası herhangi bir noktada

Sürekli `DriftAmount` düzgün çalıştıktan sonra state eklenebilir. State'ler fizik
parametrelerinin tek kaynağı olmak zorunda değil; fizik tuning'i yine `DriftAmount`
üzerinden smooth yapılır. State şunları yönetir: özel geçiş davranışları, VFX, SFX,
animasyon, scoring, kamera.

- Stable: DriftAmount 0.0 – 0.15
- DriftInitiation: 0.15 – 0.45
- Drifting: 0.45 – 1.0
- Recovery: DriftAmount düşüyor ama araç hâlâ lateral hareket taşıyor

---

## İLK İMPLEMENTASYONDA YAPMA

- Aynı anda bütün fizik değerlerini değiştirmek
- DriftEvaluator içinde AddForce kullanmak
- CarBehaviour içinde Rigidbody kullanmak
- Drift sistemine başlamadan suspension'ı tekrar yazmak
- İlk günden karmaşık state machine kurmak
- İlk günden gerçekçi lastik slip modeli yazmak
- Mouse drag angle + swipe velocity + state machine + slip angle + VFX'i aynı anda eklemek

## İLK HEDEF (Aşama 0–8 tamamlandığında biten zincir)

Mouse sert sağa çekildi → SteerInput yükseldi → araba dönmeye başladı → LateralVelocity
oluştu → DriftEvaluator DriftAmount'u yükseltti → CarBehaviour grip'i yavaşça düşürdü →
araba daha fazla yana kaydı → mouse bırakıldı → DriftAmount yavaşça düştü → grip normale
döndü → araba kontrollü şekilde toparlandı.
