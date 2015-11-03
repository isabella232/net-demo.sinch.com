using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Sinch.ServerSdk.Calling;

namespace demo.sinch.com.Models {
    public class ConferenceDetailsViewModel {
        public Conference Conference { get; set; }
        public IParticipant[] Participants { get; set; }
    }

    public class CreateConferenceModel {
        public Conference Conference { get; set; }
        public List<ConferenceAtendee> Participants { get; set; }
    }

    public class Conference {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public string ConferenceName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ConferenceEndDate { get; set; }

        [MaxLength(4)]
        public string PinCode { get; set; }

        public Guid ConferenceId { get; set; }
        public virtual List<ConferenceAtendee> Attendees { get; set; }
        public bool ValidForever { get; set; }
    }

    public class ConferenceAtendee {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class ConferenceContext : DbContext {
        public ConferenceContext() : base("DefaultConnection") {}
        public DbSet<Conference> Conferences { get; set; }
        public DbSet<ConferenceAtendee> ConferenceAtendees { get; set; }
        public DbSet<CurrentParticipant> CurrentParticipants { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Entity<Conference>()
                .HasMany(a => a.Attendees).WithOptional().WillCascadeOnDelete(true);
        }
    }

    public class CurrentParticipant {
        [Key]
        public int Id { get; set; }

        public Guid ConferenceId { get; set; }
        public string Name { get; set; }
        public DateTime Joined { get; set; }
        public string AtendeeId { get; set; }

        public async void AddParticipant(string name) {
            using (var db = new ConferenceContext())
            {
                db.CurrentParticipants.Add(new CurrentParticipant
                {
                    Name = name,
                    Joined = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
        }
    }
}