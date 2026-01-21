## UI

injector-volume-transfer-label = Об'єм: [color=white]{$currentVolume}/{$totalVolume}u[/color]
    Режим: [color=white]{$modeString}[/color] ([color=white]{$transferVolume}u[/color])
injector-volume-label = Об'єм: [color=white]{$currentVolume}/{$totalVolume}u[/color]
    Режим: [color=white]{$modeString}[/color]
injector-toggle-verb-text = Перемкнути режим ін'єктора

## Entity

injector-component-inject-mode-name = введення
injector-component-draw-mode-name = набір
injector-component-dynamic-mode-name = динамічний
injector-component-mode-changed-text = Тепер: {$mode}
injector-component-transfer-success-message = Ви переливаєтe {$amount}u в {$target}.
injector-component-transfer-success-message-self = Ви переливаєтe {$amount}u в себе.
injector-component-inject-success-message = Ви впорскуєте {$amount}u в {$target}!
injector-component-inject-success-message-self = Ви впорскуєте {$amount}u собі!
injector-component-draw-success-message = Ви набираєте {$amount}u з {$target}.
injector-component-draw-success-message-self = Ви набираєте {$amount}u з себе.

## Fail Messages

injector-component-target-already-full-message = {CAPITALIZE($target)} вже заповнен(-а/ий)!
injector-component-target-already-full-message-self = Ви вже заповнені!
injector-component-target-is-empty-message = {CAPITALIZE($target)} порожній(-ня)!
injector-component-target-is-empty-message-self = Ви порожні!
injector-component-cannot-toggle-draw-message = Занадто заповнений для набору!
injector-component-cannot-toggle-inject-message = Нічого впорскувати!
injector-component-cannot-toggle-dynamic-message = Не вдалося перемкнути динамічний режим!
injector-component-empty-message = {CAPITALIZE($injector)} порожній!
injector-component-blocked-user = Захисне спорядження заблокувало ін'єкцію!
injector-component-blocked-other = Броня {POSS-ADJ($target)} заблокувала ін'єкцію {$user}!
injector-component-cannot-transfer-message = Ви не можете перелити рідину в {$target}!
injector-component-cannot-transfer-message-self = Ви не можете перелити рідину в себе!
injector-component-cannot-inject-message = Ви не можете зробити ін'єкцію в {$target}!
injector-component-cannot-inject-message-self = Ви не можете зробити ін'єкцію собі!
injector-component-cannot-draw-message = Ви не можете набрати рідину з {$target}!
injector-component-cannot-draw-message-self = Ви не можете набрати рідину з себе!
injector-component-ignore-mobs = Цей ін'єктор може взаємодіяти лише з контейнерами!

## mob-inject doafter messages

injector-component-needle-injecting-user = Ви починаєте вводити голку.
injector-component-needle-injecting-target = {CAPITALIZE($user)} намагається встромити у вас голку!
injector-component-needle-drawing-user = Ви починаєте набирати рідину голкою.
injector-component-needle-drawing-target = {CAPITALIZE($user)} намагається використати голку, щоб набрати з вас рідину!
injector-component-spray-injecting-user = Ви починаєте готувати розпилювач.
injector-component-spray-injecting-target = {CAPITALIZE($user)} намагається прикласти до вас розпилювач!

## Target Popup Success messages
injector-component-feel-prick-message = Ви відчуваєте легкий укол!
