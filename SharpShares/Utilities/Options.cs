﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SharpShares.Utilities
{
    class Options
    {
        //default values off all arguments
        public class Arguments
        {
            public bool help = false;
            public bool stealth = false;
            public bool validate = false;
            public bool verbose = false;
            public int threads = 25;
            public List<string> filter = new List<string> { "SYSVOL", "NETLOGON", "IPC$", "PRINT$" };
            public string dc = null;
            public string domain = null;
            public string ldap = null;
            public string ou = null;
            public string outfile = null;
            public List<string> targets = null;
            public int sleep = 1;
            public int jitter = 5;
            public bool spider = false;
            public List<string> juicy = new List<string> { "password" };
        }
        public static Dictionary<string, string[]> ParseArgs(string[] args)
        {
            Dictionary<string, string[]> result = new Dictionary<string, string[]>();
            //these boolean variables aren't passed w/ values. If passed, they are "true"
            string[] booleans = new string[] { "/stealth", "/validate", "/verbose" , "/spider" };
            var argList = new List<string>();
            foreach (string arg in args)
            {
                //delimit key/value of arguments by ":"
                string[] parts = arg.Split(":".ToCharArray(), 2);
                argList.Add(parts[0]);

                //boolean variables
                if (parts.Length == 1)
                {
                    result[parts[0]] = new string[] { "true" };
                }
                if (parts.Length == 2)
                {
                    result[parts[0]] = new string[] { parts[1] };
                }
            }
            return result;
        }
        public static Arguments ArgumentValues(Dictionary<string, string[]> parsedArgs)
        {
            Arguments arguments = new Arguments();
            if (parsedArgs.ContainsKey("/dc"))
            {
                arguments.dc = parsedArgs["/dc"][0];
            }
            if (parsedArgs.ContainsKey("/domain"))
            {
                arguments.domain = parsedArgs["/domain"][0];
            }
            if (parsedArgs.ContainsKey("/filter"))
            {
                arguments.filter = parsedArgs["/filter"][0].ToUpper().Split(',').ToList();
            }
            if (parsedArgs.ContainsKey("/ldap"))
            {
                arguments.ldap = parsedArgs["/ldap"][0];
            }
            if (parsedArgs.ContainsKey("/ou"))
            {
                arguments.ou = parsedArgs["/ou"][0];
            }
            if (parsedArgs.ContainsKey("/outfile"))
            {
                arguments.outfile = parsedArgs["/outfile"][0];
            }
            if (parsedArgs.ContainsKey("/stealth"))
            {
                arguments.stealth = Convert.ToBoolean(parsedArgs["/stealth"][0]);
            }
            if (parsedArgs.ContainsKey("/targets"))
            {
                arguments.targets = parsedArgs["/targets"][0].Split(',').ToList();
            }
            if (parsedArgs.ContainsKey("/threads"))
            {
                arguments.threads = Convert.ToInt32(parsedArgs["/threads"][0]);
            }
            if (parsedArgs.ContainsKey("/validate"))
            {
                arguments.validate = Convert.ToBoolean(parsedArgs["/validate"][0]);
            }
            if (parsedArgs.ContainsKey("/verbose"))
            {
                arguments.verbose = Convert.ToBoolean(parsedArgs["/verbose"][0]);
            }
            if (parsedArgs.ContainsKey("/spider"))
            {
                arguments.spider = Convert.ToBoolean(parsedArgs["/spider"][0]);
            }
            if (parsedArgs.ContainsKey("/juicy"))
            {
                arguments.juicy = parsedArgs["/juicy"][0].ToLower().Split(',').ToList();
            }
            if (parsedArgs.ContainsKey("/sleep"))
            {
                arguments.sleep = Convert.ToInt32(parsedArgs["/sleep"][0]);
            }
            if (parsedArgs.ContainsKey("/jitter"))
            {
                arguments.jitter = Convert.ToInt32(parsedArgs["/jitter"][0]);
            }
            if (parsedArgs.ContainsKey("help"))
            {
                Usage();
                arguments = null;
            }
            // if no ldap or ou filter specified, search all enabled computer objects
            if (!parsedArgs.ContainsKey("/ldap") && !parsedArgs.ContainsKey("/ou") && !parsedArgs.ContainsKey("/targets"))
            {
                Console.WriteLine("[!] Must specify hosts using one of the following arguments: /ldap /ou /targets");
                PrintOptions(arguments);
                Utilities.Options.Usage();
                arguments = null;
            }
            return arguments;
        }
        public static bool PrintOptions(Utilities.Options.Arguments arguments)
        {
            bool success = true;
            Console.WriteLine("[+] Parsed Arguments:");
            if (arguments.filter != null)
                Console.WriteLine($"\tfilter: {String.Join(",", arguments.filter)}");
            else
                Console.WriteLine($"\tfilter: none");
            Console.WriteLine($"\tdc: {arguments.dc}");
            Console.WriteLine($"\tdomain: {arguments.domain}");
            Console.WriteLine($"\tldap: {arguments.ldap}");
            Console.WriteLine($"\tou: {arguments.ou}");
            Console.WriteLine($"\tstealth: {arguments.stealth.ToString()}");
            Console.WriteLine($"\tthreads: {arguments.threads.ToString()}");
            Console.WriteLine($"\tverbose: {arguments.verbose.ToString()}");
            Console.WriteLine($"\tspider: {arguments.spider.ToString()}");
            if (arguments.juicy != null)
                Console.WriteLine($"\tjuicy: {String.Join(",", arguments.juicy)}");
            else
                Console.WriteLine($"\tjuicy: none");
            if (arguments.targets != null)
                Console.WriteLine($"\ttargets: {String.Join(",", arguments.targets)}");
            else
                Console.WriteLine($"\ttargets: none");
            Console.WriteLine($"\tsleep: {arguments.sleep.ToString()}");
            Console.WriteLine($"\tjitter: {arguments.jitter.ToString()}");
            if (String.IsNullOrEmpty(arguments.outfile))
            { 
                Console.WriteLine("\toutfile: none");
            }
            else
            {
                Console.WriteLine($"\toutfile: {arguments.outfile}");
                if (!File.Exists(arguments.outfile))
                {
                    try
                    {
                        // Create a file to write to if it doesn't exist
                        using (StreamWriter sw = File.CreateText(arguments.outfile)) { };
                        Console.WriteLine($"[+] {arguments.outfile} Created");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[!] Outfile Error: {0}", ex.Message);
                        //Environment.Exit(0);
                        success = false;
                    }
                }
                else
                {
                    Console.WriteLine($"[!] {arguments.outfile} already esists. Appending to file");
                }
            }
            if (arguments.filter != null) { Console.WriteLine("[*] Excluding {0} shares", String.Join(",", arguments.filter)); }
            if (arguments.verbose) { Console.WriteLine("[*] Including unreadable shares"); }
            Console.WriteLine("[*] Starting share enumeration with thread limit of {0}", arguments.threads.ToString());
            Console.WriteLine("[R] = Readable Share\n[W] = Writeable Share\n[-] = Unauthorized Share (requires /verbose flag)\n[?] = Unchecked Share (requires /stealth flag)\n");
            
            return success;
        }
        public static void Usage()
        {
            string usageString = @"
Optional Arguments:
    /threads  - specify maximum number of parallel threads  (default=25)
    /dc       - specify domain controller to query (if not ran on a domain-joined host)
    /domain   - specify domain name (if not ran on a domain-joined host)
    /ldap     - query hosts from the following LDAP filters (default=all)
         :all - All enabled computers with 'primary' group 'Domain Computers'
         :dc  - All enabled Domain Controllers (not read-only DCs)
         :exclude-dc - All enabled computers that are not Domain Controllers or read-only DCs
         :servers - All enabled servers
         :servers-exclude-dc - All enabled servers excluding Domain Controllers or read-only DCs
    /ou       - specify LDAP OU to query enabled computer objects from
                ex: ""OU=Special Servers,DC=example,DC=local""
    /stealth  - list share names without performing read/write access checks
    /filter   - list of comma-separated shares to exclude from enumeration
                default: SYSVOL,NETLOGON,IPC$,PRINT$
    /outfile  - specify file for shares to be appended to instead of printing to std out 
    /verbose  - return unauthorized shares
    /spider   - print a list of all files existing within directories (and subdirectories) in identified shares
    /juicy    - list of comma-separated tokens to match in spidered files/folders to be reported as juicy
    /targets  - specify a comma-separated list of target hosts
    /sleep    - specify the time (in seconds) to sleep after each host is enumerated
    /jitter   - specify a jitter percentage for the sleeping pattern (0-100)
";
            Console.WriteLine(usageString);
        }
    } 
}
