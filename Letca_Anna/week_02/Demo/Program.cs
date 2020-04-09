﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Domain;
using Domain.Domain.CreateRestauratOp;
using Domain.Models;
using Infrastructure.Free;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Domain.Domain.AddMenuItemOp.AddMenuItemResult;
using static Domain.Domain.CreateMenuItemOp.CreateMenuItemResult;
using static Domain.Domain.CreateMenuOp.CreateMenuResult;
using static Domain.Domain.CreateRestauratOp.CreateRestaurantResult;

namespace Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOperations(typeof(Restaurant).Assembly);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var expr =
                from restaurantResult in RestaurantDomain.CreateRestaurant("mcdonalds")
                let restaurant = (restaurantResult as RestaurantCreated)?.Restaurant
                from menuResult in RestaurantDomain.CreateMenu(restaurant, "burgers", MenuType.Meat)
                let menu = (menuResult as MenuCreated)?.Menu
                from menuItemResult in RestaurantDomain.CreateMenuItem("carbonara", 20)
                let menuItem = (menuItemResult as MenuItemCreated)?.MenuItem
                from menuAddedResult in RestaurantDomain.AddMenuItem(menu, menuItem)
                select menuAddedResult;

            Console.WriteLine(expr);

            var interpreter = new LiveInterpreterAsync(serviceProvider);

            var result = await interpreter.Interpret(expr, Unit.Default);

            var finalResult = result.Match<bool>(OnMenuItemAdded, OnMenuItemNotAdded);
            //Assert.False(finalResult);
            Assert.True(finalResult);

            Console.WriteLine("Hello World!");
        }

        private static bool OnRestaurantNotCreated(RestaurantNotCreated arg)
        {
            return false;
        }

        private static bool OnRestaurantCreated(RestaurantCreated arg)
        {
            return true;
        }

        private static bool OnMenuItemNotCreated(MenuItemNotCreated arg)
        {
            return false;
        }

        private static bool OnMenuItemCreated(MenuItemCreated arg)
        {
            return true;
        }

        private static bool OnMenuItemNotAdded(MenuItemNotAdded arg)
        {
            return false;
        }

        private static bool OnMenuItemAdded(MenuItemAdded arg)
        {
            Console.WriteLine(arg.Menu.ToString());
            return true;
        }

    }
}
