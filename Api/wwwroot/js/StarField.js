const StarField = canvas => {
    const numberOfStars = 70;

    // Create an array to store the stars
    const stars = [];
    let lines = [];
    let target = { x: 500, y: 500 };
    const white = { r: 255, g: 255, b: 255 };
    const green = { r: 0, g: 255, b: 0 };
    const purple = { r: 255, g: 0, b: 255 };

    // Create a random number of stars
    for (let i = 0; i < numberOfStars; i++) {
        stars.push({
            x: Math.random() * canvas.width,
            y: Math.random() * canvas.height,
            radius: Math.random() * 2 + 1,
            alpha: Math.random(),
            color: white
        });
    }

    // Update the stars
    const update = (after) => {
        lines = [];

        for (const star of stars) {
            star.x += 2 * 0.1 / star.alpha + 1;

            // If the star goes off the bottom of the screen, wrap it around to the top
            // and give it a new random color
            if (star.x > canvas.width) {
                if (star.color === white) {
                    frames -= 10;
                }

                star.x = 0;
                star.y = Math.random() * canvas.height;
                star.radius = Math.random() * 2 + 1;
                star.alpha = Math.random();
                star.color = white;
            }

            if (after) {
                after(star, canvas, stars);
            }

            star.distanceToTarget = Math.sqrt((target.x - star.x) ** 2 + (target.y - star.y) ** 2);
            insertIfNearest(lines, star);
        }

        // connected stars
        for (let i = 0; i < lines.length; i++) {
            lines[i].color = purple;
        }
    };

    // Draw the stars on a canvas
    const draw = ctx => {
        for (let i = 0; i < stars.length; i++) {
            // Draw the star
            ctx.beginPath();
            ctx.arc(stars[i].x, stars[i].y, stars[i].radius, 0, Math.PI * 2);
            ctx.closePath();
            ctx.fillStyle = `rgba(${stars[i].color.r}, ${stars[i].color.g}, ${stars[i].color.b}, ${stars[i].alpha})`;
            ctx.fill();
        }

        for (let i = 0; i < lines.length; i++) {
            drawLine(ctx, lines[i], target);
        }
    };

    const setTarget = t => target = t;

    const drawLine = (ctx, star, target) => {
        // Set the starting and ending points of the line
        const x1 = star.x;
        const y1 = star.y;
        const x2 = target.x;
        const y2 = target.y;

        // Draw the line
        ctx.beginPath();
        ctx.strokeStyle = `rgba(255, 255, 255, ${100 / star.distanceToTarget})`;
        ctx.lineWidth = Math.min(3, 300 / star.distanceToTarget);
        ctx.moveTo(x1, y1);
        ctx.lineTo(x2, y2);
        ctx.stroke();
        ctx.closePath();
    };

    function insertIfNearest(array, star) {
        const limit = 4;
        const maxLength = 100;

        // Ignore the value if it's larger than the largest value in the array and we already have 5 (limit) items
        if (star.distanceToTarget > maxLength || (array.length === limit && star.distanceToTarget > array[limit - 1].distanceToTarget)) {
            return;
        }

        // Find the insertion point for the new value
        let i = 0;
        while (i < array.length && array[i].distanceToTarget < star.distanceToTarget) {
            i++;
        }

        // Insert the new value at the insertion point, or at the end of the array if we have less than 'trackers'' items
        if (array.length < limit) {
            array.splice(i, 0, star);
        } else {
            array.splice(i, 0, star);
            array.pop(); // Remove the largest value from the array
        }
    }

    // Return the public API
    return {
        update,
        draw,
        setTarget,
        lines: () => lines
    };
};

const random2d = (minX = -256, maxX = 256, minY = -256, maxY = 256) => {
    const x = Math.random() * (maxX - minX) + minX;
    const y = Math.random() * (maxY - minY) + minY;
    return { x, y };
};

const Connections = (canvas, stars) => {
    const limit = 4;
    const connections = [];


    function Update(array, star) {
        const limit = 4;
        const maxLength = 70;

        // Ignore the value if it's larger than the largest value in the array and we already have 5 (limit) items
        if (star.distanceToTarget > 100 || (array.length === limit && star.distanceToTarget > array[limit - 1].distanceToTarget)) {
            return;
        }

        // Find the insertion point for the new value
        let i = 0;
        while (i < array.length && array[i].distanceToTarget < star.distanceToTarget) {
            i++;
        }

        // Insert the new value at the insertion point, or at the end of the array if we have less than 'trackers'' items
        if (array.length < limit) {
            array.splice(i, 0, star);
        } else {
            array.splice(i, 0, star);
            array.pop(); // Remove the largest value from the array
        }
    }
};