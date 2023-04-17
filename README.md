# :orange_circle: TriviaBot :orange_circle:
A simple discord bot which creates and runs a trivia game <br/>
Bot was built using [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) for communications with the Discord api and [Open Trivia Database](https://opentdb.com/) for getting trivia questions

## How to use
Write ```!trivia``` to chat in order to initialize the bot

## How to run
Paste in your discord bot API key in the ```config.json``` file and put it in the build directory

## Known Issues
+ If the user selects all answers it will give the user a point, because the bot only checks if the correct answer is selected ```CollectReactionsAsync()``` [does not seem to work](https://github.com/DSharpPlus/DSharpPlus/issues/1542)

## To implement
- [ ] Ability to choose category
- [ ] Fix correct answer checking
- [ ] Ability to specify amount of questions
