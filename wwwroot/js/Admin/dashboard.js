//Mobile navbar (Dashboard)

const hamburgerBtn = document.getElementById("hamburgerBtn");
const mobilePanel = document.getElementById("sidebar");
hamburgerBtn.addEventListener("click", function () {
  const open = hamburgerBtn.classList.toggle("open");
  mobilePanel.classList.toggle("open");
  hamburgerBtn.setAttribute("aria-expanded", open);
});
mobilePanel.querySelectorAll("a").forEach(function (a) {
  a.addEventListener("click", function () {
    hamburgerBtn.classList.remove("open");
    mobilePanel.classList.remove("open");
  });
});

//Dashboard Chart

(() => {
  const canvas = document.getElementById("articleViewsChart");
  if (!canvas) return;

  /*
   * ============================================================
   * SAMPLE DATA — فقط برای دیدن طرز کار نمودار
   * ============================================================
   *
   * labels = مسیر محور X
   * data   = مقدار هر نقطه روی محور Y
   *
   * بعداً همین قسمت را با داده واقعی Database/API جایگزین می‌کنیم.
   */

  const sampleData = {
    "30 days": {
      labels: [
        "Sep 1",
        "Sep 3",
        "Sep 5",
        "Sep 7",
        "Sep 9",
        "Sep 11",
        "Sep 13",
        "Sep 15",
        "Sep 17",
        "Sep 19",
        "Sep 21",
        "Sep 23",
        "Sep 25",
        "Sep 27",
        "Sep 29",
      ],
      values: [
        120, 180, 145, 260, 310, 275, 390, 460, 420, 560, 610, 590, 720, 810,
        940,
      ],
    },

    "90 days": {
      labels: [
        "Jul 1",
        "Jul 8",
        "Jul 15",
        "Jul 22",
        "Jul 29",
        "Aug 5",
        "Aug 12",
        "Aug 19",
        "Aug 26",
        "Sep 2",
        "Sep 9",
        "Sep 16",
        "Sep 23",
        "Sep 30",
      ],
      values: [
        420, 510, 470, 650, 720, 680, 840, 910, 870, 1020, 1180, 1090, 1320,
        1480,
      ],
    },

    "12 months": {
      labels: [
        "Oct",
        "Nov",
        "Dec",
        "Jan",
        "Feb",
        "Mar",
        "Apr",
        "May",
        "Jun",
        "Jul",
        "Aug",
        "Sep",
      ],
      values: [
        850, 1100, 980, 1350, 1500, 1420, 1750, 2100, 1980, 2450, 2800, 3250,
      ],
    },
  };

  const getThemeColors = () => {
    const styles = getComputedStyle(document.documentElement);

    return {
      primary: styles.getPropertyValue("--color-primary").trim() || "#10B981",
      border: styles.getPropertyValue("--color-border").trim() || "#e5e7eb",
      text: styles.getPropertyValue("--color-text-muted").trim() || "#6b7280",
      bg: styles.getPropertyValue("--color-bg").trim() || "#ffffff",
    };
  };

  let colors = getThemeColors();

  const chart = new Chart(canvas, {
    type: "line",

    data: {
      labels: sampleData["30 days"].labels,

      datasets: [
        {
          label: "Article Views",

          // این آرایه مسیر واقعی نمودار را می‌سازد.
          data: sampleData["30 days"].values,

          borderColor: colors.primary,

          // فضای زیر خط
          backgroundColor: "rgba(16, 185, 129, 0.12)",

          borderWidth: 3,

          // هرچه عدد بیشتر باشد مسیر نرم‌تر می‌شود.
          tension: 0.35,

          fill: true,

          // نقطه‌های روی مسیر
          pointRadius: 4,
          pointHoverRadius: 7,
          pointBorderWidth: 2,
          pointBackgroundColor: colors.primary,
          pointBorderColor: colors.bg,
        },
      ],
    },

    options: {
      responsive: true,
      maintainAspectRatio: false,

      interaction: {
        mode: "index",
        intersect: false,
      },

      plugins: {
        legend: {
          display: false,
        },

        tooltip: {
          callbacks: {
            label: (context) => {
              return ` ${context.parsed.y.toLocaleString()} views`;
            },
          },
        },
      },

      scales: {
        x: {
          grid: {
            display: false,
          },

          ticks: {
            color: colors.text,
            font: {
              size: 11,
            },
          },

          border: {
            display: false,
          },
        },

        y: {
          beginAtZero: true,

          grid: {
            color: colors.border,
          },

          ticks: {
            color: colors.text,
            font: {
              size: 11,
            },
          },

          border: {
            display: false,
          },
        },
      },
    },
  });

  /*
   * انتخاب 30 / 90 روز / 12 ماه
   */
  const periodSelect = document.querySelector(".panel .select");

  if (periodSelect) {
    periodSelect.addEventListener("change", () => {
      const selected = sampleData[periodSelect.value];

      if (!selected) return;

      chart.data.labels = selected.labels;
      chart.data.datasets[0].data = selected.values;

      chart.update();
    });
  }

  /*
   * اگر Theme عوض شد، رنگ‌های Chart هم دوباره از CSS Variables خوانده شوند.
   */
  const themeObserver = new MutationObserver(() => {
    colors = getThemeColors();

    chart.data.datasets[0].borderColor = colors.primary;
    chart.data.datasets[0].backgroundColor = "rgba(16, 185, 129, 0.12)";
    chart.data.datasets[0].pointBackgroundColor = colors.primary;
    chart.data.datasets[0].pointBorderColor = colors.bg;

    chart.options.scales.x.ticks.color = colors.text;
    chart.options.scales.y.ticks.color = colors.text;
    chart.options.scales.y.grid.color = colors.border;

    chart.update();
  });

  themeObserver.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ["data-theme"],
  });
})();
