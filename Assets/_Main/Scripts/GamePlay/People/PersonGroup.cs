using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities.Extensions;
using Managers;
using Model;
using TriInspector;
using UnityEngine;
using Utilities;
using Grid = GridSystem.Grid;

namespace GamePlay.People
{
	public class PersonGroup : MonoBehaviour
	{
		[SerializeField, ReadOnly] private PersonType type;
		[SerializeField, ReadOnly] private List<Person> people = new List<Person>();
		public List<Person> People => people;

		private void Start()
		{
			var idleNo = Random.Range(0, 4);
			foreach (var person in people)
			{
				person.OnTap += OnPersonTapped;
				person.OnDown += HighlightPeople;
				person.OnUp += HideHighlightPeople;

				person.Animations.ChooseIdle(idleNo);
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

			SlotHolder slotHolder = null;
			if (LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator && type == LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator.ElevatorData.ElevatorType)
			{
				slotHolder = LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator;
			}
			else
			{
				slotHolder = LevelManager.Instance.CurrentLevel.HolderManager.GetFirstEmptyHolder();
				((Holder)slotHolder).CurrentPersonGroup = this;
			}

			if (path is not null && path.Count > 0)
			{
				float duration = 0;
				for (var i = 0; i < people.Count; i++)
				{
					people[i].HideHighlight();

					// Adds the position of the front person
					if (i > 0)
						pathPos.Insert(0, people[i - 1].CurrentCell.transform.position);

					duration = people[i].MoveToSlot(pathPos.ToArray(), slotHolder).Duration();
				}

				PeopleManager.Instance.WaitForPeopleMovement(people, duration + 0.5f);
			}
			else
			{
				for (var i = 0; i < people.Count; i++)
				{
					people[i].HideHighlight();
				}
			}
		}

		#region Setup

#if UNITY_EDITOR
		public void AddPerson(Person person)
		{
			type = person.PersonType;
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