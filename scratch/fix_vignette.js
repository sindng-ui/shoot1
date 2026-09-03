const fs = require('fs');

// 1. Fix GameBootstrap.cs
const gbPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Bootstrap/GameBootstrap.cs';
let gbCode = fs.readFileSync(gbPath, 'utf8');

gbCode = gbCode.replace(
  'var vignetteGo = new GameObject("PlayerDamageVignetteUI");',
  'var vignetteGo = new GameObject("PlayerDamageVignetteUI", typeof(RectTransform));'
);
fs.writeFileSync(gbPath, gbCode, 'utf8');
console.log('Fixed GameBootstrap.cs');

// 2. Fix PlayerDamageVignetteView.cs
const pvPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/PlayerDamageVignetteView.cs';
let pvCode = fs.readFileSync(pvPath, 'utf8');

pvCode = pvCode.replace(
  'public class PlayerDamageVignetteView : MonoBehaviour',
  '[RequireComponent(typeof(RectTransform))]\n    public class PlayerDamageVignetteView : MonoBehaviour'
);

pvCode = pvCode.replace(
  'var rect = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();',
  'var rect = GetComponent<RectTransform>();'
);

fs.writeFileSync(pvPath, pvCode, 'utf8');
console.log('Fixed PlayerDamageVignetteView.cs');
