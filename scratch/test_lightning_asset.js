const fs = require("fs");
const path = require("path");
const { PNG } = require("./node_modules/pngjs");

const readyPath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning_ready.png");
const rawPath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning.png");
const metaPath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning_ready.png.meta");
const loaderPath = path.resolve(__dirname, "../Assets/src/HappyShoot.View/Utils/CustomSkillSpriteLoader.cs");
const viewPath = path.resolve(__dirname, "../Assets/src/HappyShoot.View/Projectiles/MagicSkillManagerView.cs");

let report = [];
report.push("================================================================================");
report.push("  HappyShoot - Chain Lightning Asset & Visual Integration Test");
report.push("  Executed on: " + new Date().toISOString());
report.push("================================================================================");
report.push("");

let passed = 0;
let total = 0;

function assert(condition, name, details = "") {
    total++;
    if (condition) {
        passed++;
        report.push(`  [PASS] ${name}`);
        if (details) report.push(`         ${details}`);
    } else {
        report.push(`  [FAIL] ${name}`);
        if (details) report.push(`         ${details}`);
    }
}

// 1. Check raw image exists
assert(fs.existsSync(rawPath), "Raw uploaded image exists", `Path: ${rawPath}`);

// 2. Check processed image exists
assert(fs.existsSync(readyPath), "Processed transparent lightning image exists", `Path: ${readyPath}`);

// 3. Inspect processed image properties
if (fs.existsSync(readyPath)) {
    const buf = fs.readFileSync(readyPath);
    const png = PNG.sync.read(buf);
    
    assert(png.width === 1024, "Image width is 1024px", `Width: ${png.width}`);
    assert(png.height >= 250 && png.height <= 400, "Image height is optimized crop", `Height: ${png.height}`);

    // Check edge alpha (must be 0 for seamless joining)
    let leftEdgeMaxA = 0, rightEdgeMaxA = 0, topEdgeMaxA = 0, botEdgeMaxA = 0;
    let transparentCount = 0;
    let brightCount = 0;

    for (let y = 0; y < png.height; y++) {
        let leftA = png.data[(y * png.width + 0) * 4 + 3];
        let rightA = png.data[(y * png.width + (png.width - 1)) * 4 + 3];
        if (leftA > leftEdgeMaxA) leftEdgeMaxA = leftA;
        if (rightA > rightEdgeMaxA) rightEdgeMaxA = rightA;
    }

    for (let x = 0; x < png.width; x++) {
        let topA = png.data[(0 * png.width + x) * 4 + 3];
        let botA = png.data[((png.height - 1) * png.width + x) * 4 + 3];
        if (topA > topEdgeMaxA) topEdgeMaxA = topA;
        if (botA > botEdgeMaxA) botEdgeMaxA = botA;
    }

    for (let i = 0; i < png.data.length; i += 4) {
        let a = png.data[i+3];
        let r = png.data[i];
        let g = png.data[i+1];
        let b = png.data[i+2];
        if (a === 0) transparentCount++;
        if (a > 100 && (r > 150 || g > 150 || b > 200)) brightCount++;
    }

    assert(leftEdgeMaxA === 0 && rightEdgeMaxA === 0, "Horizontal edge alphas are 0 (seamless soft joining)", `Left: ${leftEdgeMaxA}, Right: ${rightEdgeMaxA}`);
    assert(topEdgeMaxA === 0 && botEdgeMaxA === 0, "Vertical edge alphas are 0 (no hard edge clipping)", `Top: ${topEdgeMaxA}, Bottom: ${botEdgeMaxA}`);
    assert(transparentCount > 0, "Transparent pixels exist (black background removed)", `Transparent px: ${transparentCount} (${(transparentCount * 100 / (png.width * png.height)).toFixed(1)}%)`);
    assert(brightCount > 10000, "Vibrant electric core & glow preserved", `Bright glowing px: ${brightCount}`);
}

// 4. Check Meta file exists
assert(fs.existsSync(metaPath), "Unity meta file exists", `Path: ${metaPath}`);

// 5. Check CustomSkillSpriteLoader.cs exists and under 500 lines
if (fs.existsSync(loaderPath)) {
    const loaderCode = fs.readFileSync(loaderPath, "utf-8");
    const loaderLines = loaderCode.split("\n").length;
    assert(loaderLines <= 500, "CustomSkillSpriteLoader is under 500 lines", `Lines: ${loaderLines}`);
    assert(loaderCode.includes("GetOrCreateChainLightningSprite"), "CustomSkillSpriteLoader provides GetOrCreateChainLightningSprite");
    assert(loaderCode.includes("WizardSkillSpriteHelper.GetOrCreateLightningBeamSprite"), "CustomSkillSpriteLoader has procedural fallback");
} else {
    assert(false, "CustomSkillSpriteLoader.cs exists");
}

// 6. Check MagicSkillManagerView.cs exists and under 500 lines
if (fs.existsSync(viewPath)) {
    const viewCode = fs.readFileSync(viewPath, "utf-8");
    const viewLines = viewCode.split("\n").length;
    assert(viewLines <= 500, "MagicSkillManagerView is under 500 lines", `Lines: ${viewLines}`);
    assert(viewCode.includes("CustomSkillSpriteLoader.GetOrCreateChainLightningSprite()"), "MagicSkillManagerView uses CustomSkillSpriteLoader");
    assert(viewCode.includes("SpawnGigastormNodeSpark(currentPos)"), "MagicSkillManagerView triggers electric spark at hit nodes");
} else {
    assert(false, "MagicSkillManagerView.cs exists");
}

let failed = total - passed;
report.push("");
report.push("================================================================================");
report.push(`  Results: ${passed} / ${total} Passed (${failed === 0 ? "100% SUCCESS" : "FAILED"})`);
report.push("================================================================================");

const outReport = report.join("\n");
console.log(outReport);

const docDest = path.resolve(__dirname, "../docs/chain_lightning_asset_test_result.txt");
fs.writeFileSync(docDest, outReport, "utf-8");
console.log("Saved test result to:", docDest);
