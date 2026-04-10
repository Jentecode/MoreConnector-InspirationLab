using System;
using System.Security.RightsManagement;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MoreConnector

public static class MoreConnector
{
    public static List<User> Users { get; set; } = new List<User>();
    public static List<Event> Events { get; set; } = new List<Event>();
    public static List<Post> Posts { get; set; } = new List<Post>();
    public static List<Groep> Groepen { get; set; } = new List<Groep>();
    public static List<Comment> Comments { get; set; } = new List<Comment>();
    public static List<Message> Messages { get; set; } = new List<Message>();
    public static List<Relationship> Relationships { get; set; } = new List<Relationship>();
}
