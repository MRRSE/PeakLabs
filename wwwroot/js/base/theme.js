// Reveal in animation on Y , by scroll
const revealElements = document.querySelectorAll(".reveal");

const observer = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.classList.add("in");
    }
  });
});

revealElements.forEach((element) => {
  observer.observe(element);
});


// Reveal in animation on X , by scroll
const revealElementsX = document.querySelectorAll(".reveal-X");

const observerX = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.classList.add("in");
    }
  });
});

revealElementsX.forEach((element) => {
  observer.observe(element);
});


// Reveal in animation on X Revers , by scroll
const revealElementsXRev = document.querySelectorAll(".reveal-X-Rev");

const observerXRev = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.classList.add("in");
    }
  });
});

revealElementsXRev.forEach((element) => {
  observer.observe(element);
});


// Counter animation
(() => {
    const peaklabsCounters = document.querySelectorAll(".counter");

    const peaklabsCounterObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach((entry) => {
            if (!entry.isIntersecting) return;

            const peaklabsCounter = entry.target;
            const peaklabsTarget = Number(peaklabsCounter.dataset.target);

            let peaklabsCurrent = 0;

            const peaklabsDuration = 1000;
            const peaklabsStartTime = performance.now();

            const peaklabsAnimateCounter = (currentTime) => {
                const peaklabsProgress = Math.min(
                    (currentTime - peaklabsStartTime) / peaklabsDuration,
                    1
                );

                peaklabsCurrent = Math.floor(
                    peaklabsProgress * peaklabsTarget
                );

                peaklabsCounter.textContent = peaklabsCurrent;

                if (peaklabsProgress < 1) {
                    requestAnimationFrame(peaklabsAnimateCounter);
                }
            };

            requestAnimationFrame(peaklabsAnimateCounter);

            observer.unobserve(peaklabsCounter);
        });
    });

    peaklabsCounters.forEach((counter) => {
        peaklabsCounterObserver.observe(counter);
    });
})();