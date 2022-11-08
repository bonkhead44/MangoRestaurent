﻿namespace Mango.Services.ShoppingCartAPI.Models.Dtos
{
    public class ResponseDto
    {
        public bool IsSuccess { get; set; } = true;
        public object Result { get; set; }
        public string DispalyMessage { get; set; } = "";
        public List<string> ErrorMessages { get; set; }
    }
}