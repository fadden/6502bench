/*
 * Copyright 2018 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Diagnostics;

namespace Asm65 {
    public static class Address {
        /// <summary>
        /// Converts a 16- or 24-bit address to a string.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static string AddressToString(int addr, bool always24) {
            if (!always24 && addr < 65536) {
                return addr.ToString("x4");
            } else {
                return (addr >> 16).ToString("x2") + "/" + (addr & 0xffff).ToString("x4");
            }
        }

        /// <summary>
        /// Parses and validates a 16- or 24-bit address, expressed in hexadecimal.  Bits
        /// 16-23 may be specified with a slash.
        /// 
        /// The following all evaluate to the same thing: 1000, $1000, 0x1000, 00/1000.
        /// </summary>
        /// <param name="addrStr">String to validate.</param>
        /// <param name="max">Maximum valid address value.</param>
        /// <param name="addr">Integer form.</param>
        /// <returns>True if the address is valid.</returns>
        public static bool ParseAddress(string addrStr, int max, out int addr) {
            string trimStr = addrStr.Trim();       // strip whitespace
            if (trimStr.Length < 1) {
                addr = -1;
                return false;
            }
            if (trimStr[0] == '$') {
                trimStr = trimStr.Remove(0, 1);
            }

            int slashIndex = trimStr.IndexOf('/');
            try {
                if (slashIndex < 0) {
                    addr = Convert.ToInt32(trimStr, 16);
                    if (addr < 0 || addr > max) {
                        Debug.WriteLine("Simple value out of range");
                        addr = -1;
                        return false;
                    }
                } else {
                    string[] splitStr = trimStr.Split('/');
                    if (splitStr.Length == 2) {
                        int addr1 = Convert.ToInt32(splitStr[0], 16);
                        int addr2 = Convert.ToInt32(splitStr[1], 16);
                        addr = (addr1 << 16) | addr2;
                        // Check components separately to catch overflow.
                        if (addr1 < 0 || addr1 > 255 || addr2 < 0 || addr2 > 65535 ||
                                addr > max) {
                            Debug.WriteLine("Slash value out of range");
                            addr = -1;
                            return false;
                        }
                    } else {
                        addr = -1;
                    }
                }
            } catch (Exception) {
                // Thrown from Convert.ToInt32
                //Debug.WriteLine("ValidateAddress: conversion of '" + addrStr + "' failed: " +
                //    ex.Message);
                addr = -1;
                return false;
            }

            //Debug.WriteLine("Conv " + addrStr + " --> " + addr.ToString("x6"));
            return true;
        }
    }
}
