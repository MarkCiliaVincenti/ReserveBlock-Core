﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ReserveBlockCore.Data;
using ReserveBlockCore.EllipticCurve;

namespace ReserveBlockCore.Models
{
    public class Account
    {
        public long Id { get; set; }
        public string PrivateKey { set; get; }
        public string PublicKey { set; get; }
        public string Address { get; set; }
        public string? ADNR { get; set; }
        public decimal Balance { get; set; }
        public bool IsValidating { get; set; }

        public Account Build()
        {
            var account = new Account();
            account = AccountData.CreateNewAccount();
            return account;
        }

        public async static Task<Account> Restore(string privKey)
        {
            Account account = await AccountData.RestoreAccount(privKey);
            return account;
        }
        public static async Task AddAdnrToAccount(string address, string name)
        {
            var accounts = AccountData.GetAccounts();
            var account = accounts.FindOne(x => x.Address == address);

            if(account != null)
            {
                account.ADNR = name.ToLower();
                accounts.UpdateSafe(account);
            }
        }
        public static async Task DeleteAdnrFromAccount(string address)
        {
            var accounts = AccountData.GetAccounts();
            var account = accounts.FindOne(x => x.Address == address);

            if (account != null)
            {
                account.ADNR = null;
                accounts.UpdateSafe(account);
            }
        }
        public static async Task TransferAdnrToAccount(string fromAddress, string toAddress)
        {
            var adnrs = Adnr.GetAdnr();
            if(adnrs != null)
            {
                var adnr = adnrs.FindOne(x => x.Address == toAddress); //state trei has alrea
                if (adnr != null)
                {
                    var accounts = AccountData.GetAccounts();
                    var account = accounts.FindOne(x => x.Address == toAddress);

                    if (account != null)
                    {
                        account.ADNR = adnr.Name;
                        accounts.UpdateSafe(account);
                    }

                }
            }
            
        }
    }


}
