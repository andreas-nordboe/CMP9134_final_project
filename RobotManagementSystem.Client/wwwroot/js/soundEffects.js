window.robotSoundEffects = {
    play: function (soundPath) {
        const audio = new Audio(soundPath);
        audio.volume = 0.45;

        audio.play().catch((error) => {
            console.warn("Sound could not play:", soundPath, error);
        });
    }
};

window.robotCrashEffects = {
    explode: function () {
        const robotImage = document.querySelector(".robot-stuck-effect .tile-image, .robot-crashed .tile-image");

        if (!robotImage) {
            return;
        }

        const rect = robotImage.getBoundingClientRect();
        const centerX = rect.left + rect.width / 2;
        const centerY = rect.top + rect.height / 2;

        const particleCount = 36;

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement("div");

            const angle = Math.random() * Math.PI * 2;
            const distance = 70 + Math.random() * 80;

            const targetX = Math.cos(angle) * distance;
            const targetY = Math.sin(angle) * distance;

            const size = 7 + Math.random() * 8;

            particle.style.position = "fixed";
            particle.style.left = `${centerX}px`;
            particle.style.top = `${centerY}px`;
            particle.style.width = `${size}px`;
            particle.style.height = `${size}px`;
            particle.style.borderRadius = Math.random() > 0.45 ? "50%" : "2px";
            particle.style.background = Math.random() > 0.5 ? "#ff5252" : "#f97316";
            particle.style.boxShadow = "0 0 12px rgba(255, 82, 82, 0.95)";
            particle.style.pointerEvents = "none";
            particle.style.zIndex = "99999";
            particle.style.transform = "translate(-50%, -50%) scale(1)";
            particle.style.opacity = "1";
            particle.style.transition = "transform 850ms cubic-bezier(.16,.84,.44,1), opacity 850ms ease-out";

            document.body.appendChild(particle);

            requestAnimationFrame(() => {
                particle.style.transform =
                    `translate(calc(-50% + ${targetX}px), calc(-50% + ${targetY}px)) scale(0.15) rotate(${Math.random() * 360}deg)`;
                particle.style.opacity = "0";
            });

            setTimeout(() => {
                particle.remove();
            }, 950);
        }
    }
};