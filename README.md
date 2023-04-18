# :orange_circle: TriviaBot :orange_circle:
A simple discord bot which creates and runs a trivia game <br/>
Bot was built using [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) for communications with the Discord API and [Open Trivia Database](https://opentdb.com/) for getting trivia questions
<p align="center">
  <img src="https://user-images.githubusercontent.com/31960595/232910045-fafe695f-da68-47dc-a762-06196a7137b2.png">
</p>
<p align="center">
  <img src="https://user-images.githubusercontent.com/31960595/232910507-df167eda-6bb6-41b5-8d2a-7c46a528ffd0.png">
</p>

## How to use
#### Commands:
```
!trivia <number_of_questions> - Start a trivia game, number_of_questions specifies how many questions there will be, defaults to 10
!ping - pong
```

## How to run
Paste in your discord bot API key in the ```config.json``` file and put it in the build directory

## Known Issues
+ If the user selects all answers it will give the user a point, because the bot only checks if the correct answer is selected ```CollectReactionsAsync()``` [does not seem to work](https://github.com/DSharpPlus/DSharpPlus/issues/1542)

## To implement
- [ ] Ability to choose category
- [ ] Fix correct answer checking
- [x] Ability to specify amount of questions
