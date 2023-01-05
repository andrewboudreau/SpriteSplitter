const SpriteManager = canvas => {
    const sprites = [];
    let spriteCount = 0;

    // Create a Sprite class
    class Sprite {
        constructor(imageUrl, rotate, scale, position, velocity) {
            this.imageUrl = imageUrl;
            this.rotate = (rotate * Math.PI) / 180;
            this.scale = scale;
            this.position = position;
            this.velocity = velocity;
            this.facing = 0;
            this.debug = true;
            this.image = null;
            this.loaded = false;
        }

        // Load the image
        load() {
            return new Promise((resolve, reject) => {
                this.image = new Image();
                this.image.src = this.imageUrl;
                this.image.addEventListener('load', () => {
                    this.loaded = true;
                    this.size = { x: this.image.width * this.scale, y: this.image.height * this.scale };
                    console.log("loaded " + this.image.src);
                    resolve();
                });
                this.image.addEventListener('error', () => {
                    reject(new Error(`Failed to load image: ${this.imageUrl}`));
                });
            });
        }

        // Update the sprite's position based on its velocity
        update() {
            this.position.x += this.velocity.x;
            this.position.y += this.velocity.y;

            // Calculate the facing direction based on the velocity
            this.facing = this.rotate + Math.atan2(this.velocity.y, this.velocity.x);
        }

        // Draw the sprite on a canvas
        draw(ctx) {
            if (this.loaded) {
                // Save the current context state
                ctx.save();

                // Translate the context to the sprite's position
                ctx.translate(this.position.x, this.position.y);


                // Rotate the context to the sprite's facing direction
                ctx.rotate(this.facing);
                ctx.scale(this.scale, this.scale);

                if (this.debug) {
                    // Draw a border around the sprite
                    ctx.strokeStyle = 'red';
                    ctx.lineWidth = 2;
                    ctx.strokeRect(-this.image.width / 2, -this.image.height / 2, this.image.width, this.image.height);
                }

                // Draw the image on the canvas
                ctx.drawImage(this.image, -this.image.width / 2, -this.image.height / 2);

                ctx.rotate(-this.facing);
                ctx.scale(1 / this.scale, 1 / this.scale);

                if (this.debug) {
                    // Render the coordinates over the top of the sprite
                    ctx.font = '12px sans-serif';
                    ctx.textAlign = 'center';
                    ctx.fillStyle = 'white';
                    ctx.fillText(`(${Math.round(this.position.x)}, ${Math.round(this.position.y)})`, 0, 0);
                }

                // Restore the context state
                ctx.restore();
            }
        }
    }

    // Add a sprite to the manager
    const addSprite = (imageUrl, rotate = 0, scale = 1, position = { x: 0, y: 0 }, velocity = { x: 0, y: 0 }) => {
        let url = imageUrl;
        if (typeof imageUrl === 'object') {
            url = imageUrl.imageUrl;
            rotate = imageUrl.rotate || rotate;
            scale = imageUrl.scale || scale;
            position = imageUrl.position || position;
            velocity = imageUrl.velocity || velocity;
        }
        console.log("add sprite");
        const sprite = new Sprite(url, rotate, scale, position, velocity);
        sprite.load().then(() => {
            sprites.push(sprite);
            spriteCount++;
        });
    };

    // Remove a sprite from the manager
    const removeSprite = sprite => {
        const index = sprites.indexOf(sprite);
        if (index !== -1) {
            sprites.splice(index, 1);
            spriteCount--;
        }
    };

    // Update all sprites
    const update = (after) => {
        sprites.forEach(sprite => {
            sprite.update();

            if (sprite.position.x - sprite.size.x > canvas.width || sprite.position.x < 0 || sprite.position.y > canvas.height + sprite.size.y / 2 || sprite.position.y < 0) {
                sprite.position.x = 1;
                sprite.position.y = Math.random() * canvas.height;

                sprite.velocity.x = Math.random() * 3;
                sprite.velocity.y = Math.random() * 1;
            }

            if (after) {
                after(sprite, canvas, sprites);
            }
        });
    };

    // Draw all sprites on a canvas
    const draw = ctx => {
        sprites.forEach(sprite => sprite.draw(ctx));
    };

    const debug = on => { this.debug = on; }

    // Get the number of sprites
    const count = () => spriteCount;

    // Return the public API
    return {
        addSprite,
        removeSprite,
        update,
        draw,
        count,
        debug
    };
};

/*
// Example usage:

// Add a sprite to the manager
SpriteManager.addSprite(
    'https://example.com/image.png',
    Math.PI / 4, // 45 degrees
    { x: 100, y: 100 },
    { x: 1, y: 1 }
);

// Update and draw the sprites on a canvas
const canvas = document.createElement('canvas');
const ctx = canvas.getContext('2d');
SpriteManager.update();
SpriteManager.draw(ctx);

// Remove a sprite from the manager
SpriteManager.removeSprite(sprite);

// Get the number of sprites
console.log(SpriteManager.count()); // 0

*/