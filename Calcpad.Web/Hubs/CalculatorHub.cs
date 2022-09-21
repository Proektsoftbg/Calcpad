﻿using Calcpad.web.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Calcpad.web.Hubs
{
    public class CalculatorHub : Hub
    {
        private readonly ICalculatorService _calc;

        public CalculatorHub(ICalculatorService calc)
        {
            _calc = calc;
        }

        public async Task Parse(string code, int deg, bool complex)
        {
            string equation = await _calc.ParseAsync(code, GetSettings(deg, complex));
            await Clients.Caller.SendAsync("ShowEquation", equation);
        }


        public async Task Calculate(string code, int deg, bool complex)
        {
            string result = await _calc.CalculateAsync(code, GetSettings(deg, complex));
            await Clients.Caller.SendAsync("ShowResult", result);
        }

        private Core.MathSettings GetSettings(int deg, bool complex) =>
            new()
            {
                IsComplex = complex,
                Degrees = deg,
                Decimals = 8
            };
    }
}
