mergeInto(LibraryManager.library, {
    YandexInitSDK: function () {
        if (typeof YaGames === 'undefined') {
            console.error("❌ YaGames SDK не найден!");
            return;
        }

        YaGames.init().then(sdk => {
            window.ysdk = sdk;
            console.log("✅ Yandex SDK initialized");
        }).catch(err => console.error("❌ SDK init failed:", err));
    },

    YandexAuthPlayer: function () {
        if (!window.ysdk) {
            console.error("❌ SDK not initialized");
            return;
        }

        ysdk.getPlayer().then(p => {
            console.log("✅ Player authorized:", p.getName());
            alert("Вы вошли как " + p.getName());
        }).catch(() => {
            console.warn("⚠ Player not authorized, opening auth dialog...");
            ysdk.auth.openAuthDialog()
                .then(() => {
                    console.log("✅ Player authorized via dialog");
                    alert("Авторизация успешна!");
                })
                .catch(() => {
                    console.warn("❌ Player refused auth");
                    alert("Вы отказались от авторизации");
                });
        });
    },

    YandexSendScore: function (leaderboard, score) {
        if (!window.ysdk) {
            console.error("❌ SDK not initialized");
            return;
        }

        const name = UTF8ToString(leaderboard);
        ysdk.getPlayer().then(() => {
            ysdk.leaderboards.setScore(name, score)
                .then(() => {
                    console.log(`✅ Score ${score} sent to ${name}`);
                    alert("Результат отправлен: " + score);
                })
                .catch(err => {
                    console.error("❌ setScore error:", err);
                    alert("Ошибка при отправке результата: " + err.message);
                });
        }).catch(() => {
            console.warn("⚠ Not authorized, opening auth...");
            ysdk.auth.openAuthDialog()
                .then(() => {
                    ysdk.leaderboards.setScore(name, score)
                        .then(() => alert("Результат отправлен после авторизации: " + score))
                        .catch(err => alert("Ошибка после авторизации: " + err.message));
                })
                .catch(() => alert("Вы отказались от авторизации"));
        });
    },

    YandexGetLeaderboard: function (leaderboard) {
        if (!window.ysdk) {
            console.error("❌ SDK not initialized");
            return;
        }

        const name = UTF8ToString(leaderboard);

        ysdk.leaderboards.getLeaderboard(name, { quantityTop: 10 })
            .then(lb => {
                // Формируем строку топ-10: "Имя: очки\nИмя2: очки2..."
                let result = lb.scores.map(s => s.player.name + ": " + s.score).join("\n");
                // Вызываем Unity напрямую с строкой UTF-8
                window.SendMessage('YandexSDK', 'ReceiveLeaderboardData', result);
            })
            .catch(err => {
                console.error("❌ getLeaderboard error:", err);
            });
    }
});
