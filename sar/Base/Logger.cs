/* Copyright (C) 2017 Kevin Boronka
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;

using sar.Tools;

namespace sar
{
	public static class Logger
	{
		public static event LoggerEventHandler OnLog;
		public static bool LogToConsole { get; set; }

		private static readonly object loggerLock = new object();
		
		public static void Log(Exception ex)
		{
			lock (loggerLock)
			{
				Base.Program.Log(ex);
				if (OnLog != null)
				{
					try
					{
						OnLog(new LoggerEventArgs(ex));
					}
					catch
					{
						
					}
				}
				
				if (LogToConsole)
				{
					ConsoleHelper.WriteException(ex);
				}
			}
		}
		
		public static void Log(string message)
		{
			lock (loggerLock)
			{
				Base.Program.Log(message);
				if (OnLog != null)
				{
					try
					{
						OnLog(new LoggerEventArgs(message));
					}
					catch
					{
						
					}
				}
				
				if (LogToConsole)
				{
					ConsoleHelper.WriteLine(message);
				}
			}
		}
		
		public static void FlushLogs()
		{
			lock (loggerLock)
			{
				Base.Program.FlushLogs();
			}
		}
		
		public static void LogInfo()
		{
			lock (loggerLock)
			{
				Base.Program.LogInfo();
			}
		}
	}
}
