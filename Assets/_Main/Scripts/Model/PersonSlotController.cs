using System.Collections.Generic;
using GamePlay.People;
using TriInspector;
using UnityEngine;

namespace Model
{
	public abstract class PersonSlotController : MonoBehaviour
	{
		[field: Title("Holder")]
		[field: SerializeField] public PersonSlot[] Slots { get; set; }

		public virtual PersonSlot MoveToSlot(Person person)
		{
			var slot = GetFirstEmptySlot();
			slot.CurrentPerson = person;
			person.PathList.Add(slot.transform.position);
			return slot;
		}

		#region Helpers

		public virtual PersonSlot GetFirstEmptySlot()
		{
			for (int i = 0; i < Slots.Length; i++)
			{
				if (!Slots[i].CurrentPerson)
					return Slots[i];
			}

			return null;
		}

		public int GetPeopleCount()
		{
			int count = 0;
			for (int i = 0; i < Slots.Length; i++)
			{
				if (Slots[i].CurrentPerson)
					count++;
			}

			return count;
		}

		public virtual IEnumerable<Person> GetPeople()
		{
			for (int i = 0; i < Slots.Length; i++)
			{
				if (!Slots[i].CurrentPerson)
					yield return Slots[i].CurrentPerson;
			}
		}

		#endregion
	}
}