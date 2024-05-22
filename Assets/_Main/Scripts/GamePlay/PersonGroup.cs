using System.Linq;
using System.Collections.Generic;
using Fiber.Utilities.Extensions;
using TriInspector;
using UnityEngine;
using Utilities;
using Grid = GridSystem.Grid;

namespace GamePlay
{
	public class PersonGroup : MonoBehaviour
	{
		[SerializeField, ReadOnly] private PersonType type;
		[SerializeField, ReadOnly] private List<Person> people = new List<Person>();
		public List<Person> People => people;

		private void Awake()
		{
			foreach (var person in people)
			{
				person.OnTap += OnPersonTapped;
				person.OnDown += HighlightPeople;
				person.OnUp += HideHighlightPeople;
			}
		}

		private void HighlightPeople()
		{
			for (var i = 0; i < people.Count; i++)
				people[i].ShowHighlight();
		}

		private void HideHighlightPeople()
		{
			for (var i = 0; i < people.Count; i++)
				people[i].HideHighlight();
		}

		private void OnPersonTapped()
		{
			var leader = people[0];
			var path = leader.CheckPath();
			var pathPos = path?.Select(x => x.transform.position).ToList();

			//TODO: Check whether they go to goal or holder
			//

			for (var i = 0; i < people.Count; i++)
			{
				people[i].HideHighlight();

				if (path is not null && path.Count > 0)
				{
					// Adds the position of the front person
					if (i > 0)
						pathPos.Insert(0, people[i - 1].CurrentCell.transform.position);

					people[i].MoveToSlot(pathPos.ToArray(), null /**/);
				}
			}
		}

		#region Setup

#if UNITY_EDITOR
		public void AddPerson(Person person)
		{
			person.transform.SetParent(transform);
			people.Add(person);
		}

		public void SetupGroup()
		{
			// Find leader
			Person leader = null;
			foreach (var person in people)
			{
				var neighbours = Grid.Instance.GetNeighbours(Grid.Instance.GridCells[person.Coordinates.x, person.Coordinates.y]);
				if (neighbours.Count.Equals(1))
				{
					var newCoor = person.Coordinates + person.Direction.GetDirectionVector();
					var cell = Grid.Instance.TryToGetCell(newCoor);
					// If there is no person in the direction of this person looking then it is the leader
					if (cell && !cell.CurrentPerson)
					{
						leader = person;
						break;
					}
				}
			}

			if (!leader) return;

			// Sort people in a line
			var tempPeopleList = new List<Person>(people.Count) { leader };
			var nextPerson = leader;
			for (var i = 0; i < people.Count; i++)
			{
				var neighbours = Grid.Instance.GetNeighbours(Grid.Instance.GridCells[nextPerson.Coordinates.x, nextPerson.Coordinates.y]);
				foreach (var neighbour in neighbours)
				{
					if (neighbour.CurrentPerson)
						tempPeopleList.AddIfNotContains(neighbour.CurrentPerson);
				}
			}

			people = tempPeopleList;
		}
#endif

		#endregion
	}
}