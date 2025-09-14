namespace Locate64.Engine.Utils
{
	public static class DosDateTimeDecoder
	{
		public static DateTime DecodeDosDateTime(uint value, byte eraMultiplier = 0)
		{
			if (value == 0)
				return new DateTime(1970, 1, 1); // no timestamp stored

			// Split into low/high words
			ushort date = (ushort)(value & 0xFFFF);         // low word = date
			ushort time = (ushort)(value >> 16 & 0xFFFF); // high word = time

			// Decode date
			int day = date & 0x1F;              // 1–31
			int month = date >> 5 & 0x0F;       // 1–12
			int year = (date >> 9 & 0x7F) + 1980 + eraMultiplier * 128;

			// Decode time
			int seconds = (time & 0x1F) * 2;      // 0–58, 2-sec steps
			int minutes = time >> 5 & 0x3F;     // 0–59
			int hours = time >> 11 & 0x1F;    // 0–23

			try
			{
				return new DateTime(year, month, day, hours, minutes, seconds);
			}
			catch
			{
				return new DateTime(1970, 1, 1); // invalid field
			}
		}
	}
}