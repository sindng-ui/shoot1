
const fs = require("fs");
const path = require("path");
const { PNG } = require("pngjs");

const rawDir = "/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Monsters/Raw";
const outBaseDir = "/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Monsters";

const monsters = [
  { raw: "boss1.png", sub: "Boss1", name: "boss1.png" },
  { raw: "boss2.png", sub: "Boss2", name: "boss2.png" },
  { raw: "skeleton_new.png", sub: "Skeleton", name: "skeleton.png" },
];

function processMonster(m) {
  return new Promise((resolve, reject) => {
    const rawPath = path.join(rawDir, m.raw);
    const outDir = path.join(outBaseDir, m.sub);
    const outPath = path.join(outDir, m.name);

    if (!fs.existsSync(outDir)) fs.mkdirSync(outDir, { recursive: true });

    fs.createReadStream(rawPath)
      .pipe(new PNG({ filterType: 4 }))
      .on("parsed", function () {
        const width = this.width;
        const height = this.height;
        const data = this.data;

        const isBg = new Uint8Array(width * height);
        const queue = [];

        // 1. Initial seed for BFS FloodFill (edges of image)
        function isWhiteLike(idx) {
          const r = data[idx];
          const g = data[idx + 1];
          const b = data[idx + 2];
          // Background threshold: AI background is nearly pure white
          return r >= 220 && g >= 220 && b >= 220;
        }

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

        // 2. Aggressive Defringing & Erosion on boundary pixels:
        // Any pixel directly touching the background that is light/whitish (anti-aliasing fringe) is marked as BG
        const isErodedBg = new Uint8Array(isBg);
        for (let y = 1; y < height - 1; y++) {
          for (let x = 1; x < width - 1; x++) {
            const pIdx = y * width + x;
            if (isBg[pIdx]) continue;

            // Check if adjacent to background
            const touchesBg = isBg[pIdx - 1] || isBg[pIdx + 1] || isBg[pIdx - width] || isBg[pIdx + width]
              || isBg[pIdx - width - 1] || isBg[pIdx - width + 1] || isBg[pIdx + width - 1] || isBg[pIdx + width + 1];

            if (touchesBg) {
              const bIdx = pIdx * 4;
              const r = data[bIdx];
              const g = data[bIdx + 1];
              const b = data[bIdx + 2];
              const brightness = (r + g + b) / 3;
              const colorDiff = Math.max(r, g, b) - Math.min(r, g, b);

              // If it has low saturation and high brightness (white halo fringe), erode it!
              if (brightness > 180 && colorDiff < 35) {
                isErodedBg[pIdx] = 1;
              } else if (brightness > 215) {
                isErodedBg[pIdx] = 1;
              }
            }
          }
        }

        // 3. Second pass erosion for stubborn white halos (distance 2)
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

        // 4. Clear transparent background pixels and find solid character border color for Color Bleed
        // To prevent white bleeding during bilinear interpolation or mipmapping:
        // Any pixel with alpha=0 or semi-transparent MUST NOT have white (255,255,255) RGB!
        // We replace it with the nearest solid inner pixel color or a dark neutral outline color.
        for (let y = 0; y < height; y++) {
          for (let x = 0; x < width; x++) {
            const pIdx = y * width + x;
            const bIdx = pIdx * 4;

            if (isErodedBg[pIdx]) {
              data[bIdx] = 0;
              data[bIdx + 1] = 0;
              data[bIdx + 2] = 0;
              data[bIdx + 3] = 0; // Fully transparent
            } else {
              // Smooth soft anti-aliased edge (1 pixel) for clean outline without jaggedness
              const touchesBorder = (x > 0 && isErodedBg[pIdx - 1]) || (x < width - 1 && isErodedBg[pIdx + 1])
                || (y > 0 && isErodedBg[pIdx - width]) || (y < height - 1 && isErodedBg[pIdx + width]);

              if (touchesBorder) {
                const r = data[bIdx];
                const g = data[bIdx + 1];
                const b = data[bIdx + 2];
                const brightness = (r + g + b) / 3;
                // If it was a faint fringe, blend alpha cleanly
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

        // 5. Color Bleeding (Smear solid inner colors 1px into transparent borders)
        // This ensures GPU Bilinear filtering samples dark edge colors, NEVER white!
        for (let y = 1; y < height - 1; y++) {
          for (let x = 1; x < width - 1; x++) {
            const pIdx = y * width + x;
            const bIdx = pIdx * 4;

            if (data[bIdx + 3] === 0) {
              // Check if adjacent to an opaque character pixel
              const neighbors = [pIdx - 1, pIdx + 1, pIdx - width, pIdx + width];
              for (const n of neighbors) {
                const nByte = n * 4;
                if (data[nByte + 3] > 180) {
                  // Bleed RGB into this transparent pixel
                  data[bIdx] = data[nByte];
                  data[bIdx + 1] = data[nByte + 1];
                  data[bIdx + 2] = data[nByte + 2];
                  // Keep alpha = 0!
                  break;
                }
              }
            }
          }
        }

        this.pack()
          .pipe(fs.createWriteStream(outPath))
          .on("finish", () => {
            console.log();
            resolve();
          })
          .on("error", reject);
      })
      .on("error", reject);
  });
}

async function run() {
  console.log("Starting Advanced Defringing & White Halo Removal on 8 Monsters...");
  for (const m of monsters) {
    await processMonster(m);
  }
  console.log("ALL 8 MONSTERS SUCCESSFULLY DEFRINGED!");
}

run();
