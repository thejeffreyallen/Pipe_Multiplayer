
namespace FrostyP_Game_Manager
{
	/// <summary>
	/// Represents A Custom Camera Setting
	/// </summary>
	public class CameraSetting
	{

		public string nameofsetting;
		public float valuefloat= 0;
		public int valueint = 0;

		/// <summary>
		/// Give a name and an Int value to be stored. The Name should match an existing public member in a FrostyPGameManager to load and be matched
		/// </summary>
		/// <param name="NameofMembertobesaved"></param>
		/// <param name="TheINTtobesaved"></param>
		public CameraSetting(string NameofMembertobesaved, int TheINTtobesaved){

			nameofsetting = NameofMembertobesaved;
			 valueint = TheINTtobesaved;
		
		
		}
		/// <summary>
		/// Give a name and an Float value to be stored. The Name should match an existing public member in a FrostyPGameManager to load and be matched
		/// </summary>
		/// <param name="NameofMembertobesaved"></param>
		/// <param name="TheFloattobeSaved"></param>
		public CameraSetting(string NameofMembertobesaved, float TheFloattobeSaved)
        {
			nameofsetting = NameofMembertobesaved;
			valuefloat = TheFloattobeSaved;
		}





	}

	
}

