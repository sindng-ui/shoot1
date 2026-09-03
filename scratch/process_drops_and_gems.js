
const fs = require("fs");
const path = require("path");
const { PNG } = require("pngjs");

const rawDir = "/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Resources/Raw";
const outBaseDir = "/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Resources";

const items = [
  { raw: "amethyst.png", sub: "Amethyst", name: "amethyst.png" },
  { raw: "emerald.png", sub: "Emerald", name: "emerald.png" },
  { raw: "exp1.png", sub: "Exp1", name: "exp1.png" },
  { raw: "exp2.png", sub: "Exp2", name: "exp2.png" },
  { raw: "ruby.png", sub: "Ruby", name: "ruby.png" },
  { raw: "goldcoin.png", sub: "goldcoin", name: "goldcoin.png" },
];

function processItem(item) {
  return new Promise((resolve, reject) => {
    const rawPath = path.join(rawDir, item.raw);
    const outDir = path.join(outBaseDir, item.sub);
    const outPath = path.join(outDir, item.name);

    fs.createReadStream(rawPath)
      .pipe(new PNG({ filterType: 4 }))
      .on("parsed", function () {
        const width = this.width;
        const height = this.height;
        const data = this.data;

        const isBg = new Uint8Array(width * height);
        const queue = [];

        function isWhiteLike(idx) {
          const r = data[idx];
          const g = data[idx + 1];
          const b = data[idx + 2];
          return r >= 225 && g >= 225 && b >= 225;
        }

        // 1. Seed from border
        for (let x = 0; x < width; x++) {
          queue.push(x, 0);
          queue.push(x, height - 1);
        }
        for (let y = 0; y < height; y++) {
          queue.push(0, y);
          queue.push(width - 1, y);
        }

        let head = 0;
        while (head < queue.length) {
          const qx = queue[head++];
          const qy = queue[head++];
          const pIdx = qy * width + qx;

          if (isBg[pIdx]) continue;
          const byteIdx = pIdx * 4;

          if (isWhiteLike(byteIdx)) {
            isBg[pIdx] = 1;

            if (qx > 0 && !isBg[pIdx - 1]) { queue.push(qx - 1, qy); }
            if (qx < width - 1 && !isBg[pIdx + 1]) { queue.push(qx + 1, qy); }
            if (qy > 0 && !isBg[pIdx - width]) { queue.push(qx, qy - 1); }
            if (qy < height - 1 && !isBg[pIdx + width]) { queue.push(qx, qy + 1); }
          }
        }

        // 2. Erosion pass 1 on boundary
        const isErodedBg = new Uint8Array(isBg);
        for (let y = 1; y < height - 1; y++) {
          for (let x = 1; x < width - 1; x++) {
            const pIdx = y * width + x;
            if (isBg[pIdx]) continue;

            const touchesBg = isBg[pIdx - 1] || isBg[pIdx + 1] || isBg[pIdx - width] || isBg[pIdx + width]
              || isBg[pIdx - width - 1] || isBg[pIdx - width + 1] || isBg[pIdx + width - 1] || isBg[pIdx + width + 1];

            if (touchesBg) {
              const bIdx = pIdx * 4;
              const r = data[bIdx];
              const g = data[bIdx + 1];
              const b = data[bIdx + 2];
              const brightness = (r + g + b) / 3;
              const colorDiff = Math.max(r, g, b) - Math.min(r, g, b);

              if (brightness > 180 && colorDiff < 35) {
                isErodedBg[pIdx] = 1;
              } else if (brightness > 215) {
                isErodedBg[pIdx] = 1;
              }
            }
          }
        }

        // 3. Erosion pass 2
        for (let y = 1; y < height - 1; y++) {
          for (let x = 1; x < width - 1; x++) {
            const pIdx = y * width + x;
            if (isErodedBg[pIdx]) continue;

            const touchesEroded = isErodedBg[pIdx - 1] || isErodedBg[pIdx + 1] || isErodedBg[pIdx - width] || isErodedBg[pIdx + width];
            if (touchesEroded) {
              const bIdx = pIdx * 4;
              const r = data[bIdx];
              const g = data[bIdx + 1];
              const b = data[bIdx + 2];
              const brightness = (r + g + b) / 3;
              if (brightness > 210) {
                isErodedBg[pIdx] = 1;
              }
            }
          }
        }

        // 4. Alpha transparency & soft edge
        for (let y = 0; y < height; y++) {
          for (let x = 0; x < width; x++) {
            const pIdx = y * width + x;
            const bIdx = pIdx * 4;

            if (isErodedBg[pIdx]) {
              data[bIdx] = 0;
              data[bIdx + 1] = 0;
              data[bIdx + 2] = 0;
              data[bIdx + 3] = 0;
            } else {
              const touchesBorder = (x > 0 && isErodedBg[pIdx - 1]) || (x < width - 1 && isErodedBg[pIdx + 1])
                || (y > 0 && isErodedBg[pIdx - width]) || (y < height - 1 && isErodedBg[pIdx + width]);

              if (touchesBorder) {
                const r = data[bIdx];
                const g = data[bIdx + 1];
                const b = data[bIdx + 2];
                const brightness = (r + g + b) / 3;
                if (brightness > 160) {
                  data[bIdx + 3] = Math.min(255, Math.max(120, Math.round(255 * (1 - (brightness - 160) / 95))));
                } else {
                  data[bIdx + 3] = 255;
                }
              } else {
                data[bIdx + 3] = 255;
              }
            }
          }
        }

        // 5. Color Bleeding (Smear RGB into 1px transparent edge to eliminate halos during filtering)
        for (let y = 1; y < height - 1; y++) {
          for (let x = 1; x < width - 1; x++) {
            const pIdx = y * width + x;
            const bIdx = pIdx * 4;

            if (data[bIdx + 3] === 0) {
              const neighbors = [pIdx - 1, pIdx + 1, pIdx - width, pIdx + width];
              for (const n of neighbors) {
                const nByte = n * 4;
                if (data[nByte + 3] > 180) {
                  data[bIdx] = data[nByte];
                  data[bIdx + 1] = data[nByte + 1];
                  data[bIdx + 2] = data[nByte + 2];
                  break;
                }
              }
            }
          }
        }

        this.pack()
          .pipe(fs.createWriteStream(outPath))
          .on("finish", () => {
            console.log("Processed " + item.name + " -> clean transparent RGBA with Color Bleed!");
            resolve();
          })
          .on("error", reject);
      })
      .on("error", reject);
  });
}

async function run() {
  console.log("Starting Defringing on 6 Gem & Exp Item Sprites...");
  for (const item of items) {
    await processItem(item);
  }
  console.log("ALL 6 GEM & EXP ITEMS SUCCESSFULLY PROCESSED!");
}

run();
