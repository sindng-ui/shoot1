const fs = require("fs");

const path = "/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Entities/MonsterTuningConfig.cs";
let code = fs.readFileSync(path, "utf8");

// Add VisualScale to MonsterStatConfig
code = code.replace(
  "public int GoldValue;",
  "public int GoldValue;\n        public float VisualScale = 1.0f;"
);

// Add GetVisualScale to MonsterTuningConfigData
const helper = ;

code = code.replace(
  "public BossStatConfig Boss = new BossStatConfig(hp: 800f, speed: 2.2f, damage: 25f, laserInterval: 8.0f, laserDamage: 25f, hazardInterval: 6.5f, hazardDamage: 18f, hazardRadius: 2.8f, exp: 30, gold: 100);\n    }",
  "public BossStatConfig Boss = new BossStatConfig(hp: 800f, speed: 2.2f, damage: 25f, laserInterval: 8.0f, laserDamage: 25f, hazardInterval: 6.5f, hazardDamage: 18f, hazardRadius: 2.8f, exp: 30, gold: 100);\n\n" + helper
);

fs.writeFileSync(path, code, "utf8");
console.log("Updated MonsterTuningConfig.cs successfully");
