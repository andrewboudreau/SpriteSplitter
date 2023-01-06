const SpriteManager = canvas => {
    const sprites = [];
    let spriteCount = 0;

    const cornerCoords = [
        [-1, -1],
        [1, -1],
        [1, 1],
        [-1, 1],
    ];
    
    const getBoundingBox = (centerX, centerY, width, height, angle) => {
        let minX = Infinity, minY = Infinity,
            maxX = -Infinity, maxY = -Infinity;

        for (const coord of cornerCoords) {
            // Rotate the point counterclockwise about the origin
            const rotX = coord[0] * width * Math.cos(angle) - coord[1] * height * Math.sin(angle);
            const rotY = coord[0] * width * Math.sin(angle) + coord[1] * height * Math.cos(angle);

            // Translate the point back to its original position
            const newX = rotX + centerX;
            const newY = rotY + centerY;

            minX = Math.min(minX, newX);
            minY = Math.min(minY, newY);
            maxX = Math.max(maxX, newX);
            maxY = Math.max(maxY, newY);
        }

        return {
            x: minX,
            y: minY,
            width: maxX - minX,
            height: maxY - minY
        };
    }
    
    // Create a Sprite class
    class Sprite {
        constructor(imageUrl, rotate, scale, position, velocity) {
            this.imageUrl = imageUrl;
            this.rotate = (rotate * Math.PI) / 180;
            this.forward = this.rotate;
            this.scale = scale;
            this.position = position;
            this.velocity = velocity;
            this.center = null;
            this.boundingBox = null;
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
                    this.size = {
                        x: this.image.width * this.scale,
                        y: this.image.height * this.scale
                    };

                    this.half = {
                        x: this.size.x / 2.0,
                        y: this.size.y / 2.0
                    };

                    this.update();

                    this.loaded = true;
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
            this.forward = this.rotate + Math.atan2(this.velocity.y, this.velocity.x);

            // Calculate the bounding box based on the size, scale, and rotation of the sprite
            this.boundingBox = getBoundingBox(this.position.x, this.position.y, this.half.x, this.half.y, this.forward);
        }

        // Draw the sprite on a canvas
        draw(ctx) {
            if (this.loaded) {
                // Save the current context state
                ctx.save();

                // Translate the context to the sprite's position
                ctx.translate(this.position.x, this.position.y);

                // Save the current context state
                ctx.save();

                // Rotate the context to the sprite's facing direction
                ctx.rotate(this.forward);
                ctx.scale(this.scale, this.scale);

                // Draw the image on the canvas
                ctx.drawImage(this.image, -this.image.width / 2, -this.image.height / 2);

                if (this.debug) {
                    // Draw a border around the sprite
                    ctx.strokeStyle = 'red';
                    ctx.lineWidth = 2;
                    ctx.strokeRect(-this.image.width / 2, -this.image.height / 2, this.image.width, this.image.height);
                }

                // Restore the context state 
                ctx.restore();

                if (this.debug) {
                    // Render the coordinates over the top of the sprite
                    ctx.font = '12px sans-serif';
                    ctx.textAlign = 'center';
                    ctx.fillStyle = 'white';
                    ctx.fillText(`(${Math.round(this.position.x)}, ${Math.round(this.position.y)})`, 0, 0);
                }

                // Restore the context state
                ctx.restore();

                if (this.debug) {
                    // Draw a circle at the center of the sprite
                    ctx.beginPath();
                    ctx.arc(this.position.x, this.position.y, 5, 0, Math.PI * 2);
                    ctx.fillStyle = 'red';
                    ctx.fill();
                    ctx.closePath();

                    // Draw a line to each corner of the bounding box
                    ctx.beginPath();
                    ctx.moveTo(this.position.x, this.position.y);
                    ctx.lineTo(this.boundingBox.x, this.boundingBox.y);
                    ctx.lineTo(this.boundingBox.x + this.boundingBox.width, this.boundingBox.y);
                    ctx.lineTo(this.boundingBox.x + this.boundingBox.width, this.boundingBox.y + this.boundingBox.height);
                    ctx.lineTo(this.boundingBox.x, this.boundingBox.y + this.boundingBox.height);
                    ctx.lineTo(this.boundingBox.x, this.boundingBox.y);
                    ctx.strokeStyle = 'green';
                    ctx.lineWidth = 2;
                    ctx.stroke();
                    ctx.closePath();

                    // Print the x,y coordinates of each corner of the bounding box
                    ctx.fillStyle = 'white';
                    ctx.font = '14px Arial';

                    ctx.fillText(`(${Math.round(this.boundingBox.x)}, ${Math.round(this.boundingBox.y)})`,
                        this.boundingBox.x,
                        this.boundingBox.y - 10);

                    ctx.fillText(
                        `(${Math.round(this.boundingBox.x + this.boundingBox.width)}, ${Math.round(this.boundingBox.y)})`,
                        this.boundingBox.x + this.boundingBox.width,
                        this.boundingBox.y - 10);

                    ctx.fillText(
                        `(${Math.round(this.boundingBox.x + this.boundingBox.width)}, ${Math.round(this.boundingBox.y + this.boundingBox.height)})`,
                        this.boundingBox.x + this.boundingBox.width,
                        this.boundingBox.y + this.boundingBox.height);

                    ctx.fillText(
                        `(${Math.round(this.boundingBox.x)}, ${Math.round(this.boundingBox.y + this.boundingBox.height)})`,
                        this.boundingBox.x,
                        this.boundingBox.y + this.boundingBox.height);
                }

            }
        }

        isPointInBoundingBox(pointX, pointY) {

            // Rotate the point counterclockwise about the bounding box center
            const rotX = (pointX - this.position.x) * Math.cos(this.forward) - (pointY - this.position.y) * Math.sin(this.forward);
            const rotY = (pointX - this.position.x) * Math.sin(this.forward) + (pointY - this.position.y) * Math.cos(this.forward);

            // Translate the point back to its original position
            const newX = rotX + this.position.x;
            const newY = rotY + this.position.y;

            var hit = (newX >= (this.position.x - this.half.x) && newX <= (this.position.x + this.half.x)) &&
                (newY >= (this.position.y - this.half.y) && newY <= (this.position.y + this.half.y));

            return hit;                
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

            if (sprite.position.x - sprite.maxlength > canvas.width || sprite.position.x < 0 || sprite.position.y > canvas.height + sprite.size.y / 2 || sprite.position.y < 0) {
                sprite.position.x = 1;
                sprite.position.y = Math.random() * canvas.height;

                sprite.velocity = random2d();
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

    const random2d = (minX = -3, maxX = 5, minY = -3, maxY = 5) => {
        const x = Math.random() * (maxX - minX) + minX;
        const y = Math.random() * (maxY - minY) + minY;
        return { x, y };
    };

    const items = () => sprites;

    // Return the public API
    return {
        addSprite,
        removeSprite,
        update,
        draw,
        count,
        debug,
        items
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