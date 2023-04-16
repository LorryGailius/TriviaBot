﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TriviaBot.External
{
    public class QuestionManager
    {
        HttpClient client;
        Response response;
        int numberOfQuestions = 10;
        public List<Question> questions;

        public QuestionManager(int num)
        {
            numberOfQuestions = num;
        }

        public QuestionManager() { }

        public async Task<int> GetQuestions()
        {
            client = new HttpClient();

            string url = "https://opentdb.com/api.php?amount=" + numberOfQuestions + "&type=multiple";
            HttpResponseMessage urlResponse = await client.GetAsync(url);

            string json = await urlResponse.Content.ReadAsStringAsync();
            response = JsonConvert.DeserializeObject<Response>(json);
            
            if (response.response_code != 0)
            {
                Console.WriteLine("Error: " + response.response_code);
                return -1;
            }

            questions = response.results;
            foreach (Question question in response.results)
            {
                question.question = WebUtility.HtmlDecode(question.question);
                question.correct_answer = WebUtility.HtmlDecode(question.correct_answer);
                List<string> decoded = new List<string>();
                foreach(string answer in question.incorrect_answers)
                {
                    decoded.Add(WebUtility.HtmlDecode(answer));
                }
                question.incorrect_answers = decoded;
            }

            return 0;
        }

        public Question GetQuestion(int questionId)
        {
            if(response.results.Count > 0 && response.results.Count - 1 >= questionId) 
            {
                return response.results[questionId];
            }
            return null;
        }

    }
}
