﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SEOlith.Contexts;

#nullable disable

namespace SEOlith.Migrations
{
    [DbContext(typeof(SeolithDbContext))]
    partial class SeolithDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SEOlith.Models.SeoAudit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("BrokenLinksCount")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("HasRobotsTxt")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasSitemap")
                        .HasColumnType("boolean");

                    b.Property<List<string>>("HeadingStructure")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("ImageCount")
                        .HasColumnType("integer");

                    b.Property<bool>("IsMobileResponsive")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastChecked")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double>("LoadTime")
                        .HasColumnType("double precision");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("WebsiteUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("LastChecked");

                    b.HasIndex("WebsiteUrl");

                    b.ToTable("seo_audits", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
