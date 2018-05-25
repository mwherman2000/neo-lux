using Neo.Lux.Core;
using Neo.Lux.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neo.Lux.Debugger
{
    public struct AVMInstruction
    {
        public int startOfs;
        public int endOfs;
        public OpCode opcode;
        public byte[] data;
    }

    public static class NeoTools
    {
        public static List<AVMInstruction> Disassemble(byte[] script)
        {
            var output = new List<AVMInstruction>();

            using (var stream = new MemoryStream(script))
            {
                using (var reader = new BinaryReader(stream))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        var entry = new AVMInstruction();
                        entry.startOfs = (int)reader.BaseStream.Position;

                        var opcode = (OpCode)reader.ReadByte();

                        entry.opcode = opcode;

                        if (opcode >= OpCode.PUSHBYTES1 && opcode <= OpCode.PUSHBYTES75)
                        {
                            var len = (byte)opcode;
                            entry.data = reader.ReadBytes(len);
                        }
                        else
                            switch (opcode)
                            {
                                // Push value
                                //case OpCode.PUSH0:

                                case OpCode.PUSHDATA1:
                                    {
                                        var len = reader.ReadByte();
                                        entry.data = reader.ReadBytes(len);
                                        break;
                                    }

                                case OpCode.PUSHDATA2:
                                    {
                                        var len = reader.ReadUInt16();
                                        entry.data = reader.ReadBytes(len);
                                        break;
                                    }


                                case OpCode.PUSHDATA4:
                                    {
                                        var len = reader.ReadInt32();
                                        entry.data = reader.ReadBytes(len);
                                        break;
                                    }

                                /*case OpCode.PUSHM1:
                                case OpCode.PUSH1:
                                case OpCode.PUSH2:
                                case OpCode.PUSH3:
                                case OpCode.PUSH4:
                                case OpCode.PUSH5:
                                case OpCode.PUSH6:
                                case OpCode.PUSH7:
                                case OpCode.PUSH8:
                                case OpCode.PUSH9:
                                case OpCode.PUSH10:
                                case OpCode.PUSH11:
                                case OpCode.PUSH12:
                                case OpCode.PUSH13:
                                case OpCode.PUSH14:
                                case OpCode.PUSH15:
                                case OpCode.PUSH16:*/

                                // Control
                                //case OpCode.NOP:

                                case OpCode.CALL:
                                case OpCode.JMP:
                                case OpCode.JMPIF:
                                case OpCode.JMPIFNOT:
                                    {
                                        int offset = reader.ReadInt16();
                                        //offset = context.InstructionPointer + offset - 3;
                                        break;
                                    }


                                /*case OpCode.CALL:
                                    InvocationStack.Push(context.Clone());
                                    context.InstructionPointer += 2;
                                    ExecuteOp(OpCode.JMP, CurrentContext);
                                    break;*/

                                /*case OpCode.RET:
                                    InvocationStack.Pop().Dispose();
                                    if (InvocationStack.Count == 0)
                                        State |= VMState.HALT;
                                    break;*/

                                case OpCode.APPCALL:
                                case OpCode.TAILCALL:
                                    {
                                        byte[] script_hash = reader.ReadBytes(20);
                                        entry.data = script_hash;
                                        break;
                                    }

                                case OpCode.SYSCALL:
                                    {
                                        entry.data = reader.ReadVarBytes(252);
                                        break;
                                    }

                                // Stack ops
                                /*case OpCode.DUPFROMALTSTACK:
                                case OpCode.TOALTSTACK:
                                case OpCode.FROMALTSTACK:
                                case OpCode.XDROP:
                                case OpCode.XSWAP:
                                case OpCode.XTUCK:
                                case OpCode.DEPTH:
                                case OpCode.DROP:
                                case OpCode.DUP:
                                case OpCode.NIP:
                                case OpCode.OVER:
                                case OpCode.PICK:
                                case OpCode.ROLL:
                                case OpCode.ROT:
                                case OpCode.SWAP:
                                case OpCode.TUCK:
                                case OpCode.CAT:
                                case OpCode.SUBSTR:
                                case OpCode.LEFT:
                                case OpCode.RIGHT:
                                case OpCode.SIZE:
                                    */


                                // Bitwise logic
                                /*case OpCode.INVERT:
                                case OpCode.AND:
                                case OpCode.OR:
                                case OpCode.XOR:
                                case OpCode.EQUAL:
        |                            */



                                // Numeric
                                /*case OpCode.INC:
                                case OpCode.DEC:
                                case OpCode.SIGN:
                                case OpCode.NEGATE:
                                case OpCode.ABS:
                                case OpCode.NOT:
                                case OpCode.NZ:
                                case OpCode.ADD:
                                case OpCode.SUB:
                                case OpCode.MUL:
                                case OpCode.DIV:
                                case OpCode.MOD:
                                case OpCode.SHL:
                                case OpCode.SHR:
                                case OpCode.BOOLAND:
                                case OpCode.BOOLOR:
                                case OpCode.NUMEQUAL:
                                case OpCode.NUMNOTEQUAL:
                                case OpCode.LT:
                                case OpCode.GT:
                                case OpCode.LTE:
                                case OpCode.GTE:
                                case OpCode.MIN:
                                case OpCode.MAX:
                                case OpCode.WITHIN:
                                    */

                                // Crypto
                                /*
                                case OpCode.SHA1:
                                case OpCode.SHA256:
                                case OpCode.HASH160:
                                case OpCode.HASH256:
                                case OpCode.CHECKSIG:
                                case OpCode.CHECKMULTISIG:
                                case OpCode.ARRAYSIZE:
                                case OpCode.PACK:
                                case OpCode.UNPACK:
                                case OpCode.PICKITEM:
                                case OpCode.SETITEM:
                                case OpCode.NEWARRAY:
                                case OpCode.NEWSTRUCT:
                                */

                                // Exceptions
                                /*case OpCode.THROW:
                                case OpCode.THROWIFNOT:
                                */

                                default:
                                    {
                                        if (!Enum.IsDefined(typeof(OpCode), opcode))
                                        {
                                            var s = ((byte)opcode).ToString();
                                            throw new Exception("Invalid opcode " + s);
                                        }

                                        break;
                                    }
                            }

                        entry.endOfs = (int)(reader.BaseStream.Position - 1);
                        output.Add(entry);
                    }

                }
            }

            return output;
        }

    }
}
