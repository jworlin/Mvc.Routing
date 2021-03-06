using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace Mvc.Routing
{
    public static class Routing
    {
        public static void Register(Assembly controllerAssembly)
        {
            Register(new [] { controllerAssembly});
        }

        public static IEnumerable<RouteInfo> GetRoutes(Assembly controllerAssembly)
        {
            return
                GetControllersThatWantRouting(new[] { controllerAssembly }).Select(GetRouteInfoFor).SelectMany(
                    routeInfo => routeInfo);
        }
        
        public static IEnumerable<RouteInfo> GetRoutes(Assembly[] controllerAssemblies)
        {
            return
                GetControllersThatWantRouting(controllerAssemblies).Select(GetRouteInfoFor).SelectMany(
                    routeInfo => routeInfo);
        } 

        public static void Register(Assembly[] controllerAssemblies)
        {
            foreach (var route in GetRoutes(controllerAssemblies))
            {
                if (RouteTable.Routes[route.GetRouteName()] == null)
                    RouteTable.Routes.MapRoute(route.GetRouteName(), route.Route,
                                           new {controller = route.ControllerName, action = route.ActionName});
            }
        }

        static IEnumerable<Type> GetControllersThatWantRouting(IEnumerable<Assembly> controllerAssemblies)
        {
            return controllerAssemblies.SelectMany(a => a.GetTypes().Where(x => typeof(Controller).IsAssignableFrom(x)));
        }

        static IEnumerable<RouteInfo> GetRouteInfoFor(Type controllerType)
        {
            var routeInfo = new List<RouteInfo>();
            var methods = controllerType.GetMethods();
            foreach (var methodInfo in methods)
            {
                var customAttributes = methodInfo.GetCustomAttributes(true).Where(IsOurAttribute);
                routeInfo.AddRange(customAttributes.Select(customAttribute => RouteInfoFactory.CreateFrom(controllerType, methodInfo.Name, customAttribute)));
            }
            return routeInfo.OrderByDescending(x => x.Route);
        }


        static bool IsOurAttribute(object customAttribute)
        {
            var attr = (customAttribute as BaseRouteAttribute);
            if (attr != null)
            {
                return true;
            }
            return customAttribute.GetType() == typeof(RouteAttribute);
        }
    }
}