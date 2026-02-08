# Displayed as initiator of vote when no user creates the vote
ui-vote-initiator-server = Сервер

## Default.Votes

ui-vote-restart-title = Перезапуск
ui-vote-restart-succeeded = Успішно перезапущено голосування!
ui-vote-restart-failed = Неуспішний перезапуск: (need { TOSTRING($ratio, "P0") }).
ui-vote-restart-fail-not-enough-ghost-players = Помилка голосування за перезапуск: для ініціювання голосування за перезапуск потрібна мінімум { $ghostPlayerRequirement }% гравців-привидів. Наразі не вистачає гравців-привидів.
ui-vote-restart-yes = Так
ui-vote-restart-no = Ні
ui-vote-restart-abstain = Утриматись

ui-vote-gamemode-title = Наступний режим
ui-vote-gamemode-tie = Нічия у виборі режиму! Підбір... { $picked }
ui-vote-gamemode-win = У голосуванні переміг режим «{ $winner }»!

ui-vote-map-title = Наступна мапа
ui-vote-map-tie = Нічия у голосуванні за мапу! Вибираю... { $picked }
ui-vote-map-win = { $winner } перемогла у голосуванні за мапу!
ui-vote-map-notlobby = Голосування за карти дійсне лише у лобі перед початком раунду!
ui-vote-map-notlobby-time = Голосування за картку дійсне тільки в лобі до початку раунду, до якого залишилося { $time }!
ui-vote-map-invalid = { $winner } став недійсним після голосування за мапу! Він не буде обраний!

# Votekick votes
ui-vote-votekick-unknown-initiator = Гравець
ui-vote-votekick-unknown-target = Невідомий гравець
ui-vote-votekick-title = { $initiator } почав голосування за кік гравця: { $targetEntity }. причина: { $reason }
ui-vote-votekick-yes = Так
ui-vote-votekick-no = Ні
ui-vote-votekick-abstain = Утриматися
ui-vote-votekick-success = Голосування за кік { $target } успішне. Причина кіку: { $reason }
ui-vote-votekick-failure = Голосування за кік { $target } провалено. Причина кіку: { $reason }
ui-vote-votekick-not-enough-eligible = Недостатньо гравців, які мають право голосу онлайн, щоб розпочати голосування: { $voters }/{ $requirement }
ui-vote-votekick-server-cancelled = Голосування за виключення { $target } було скасовано сервером.
