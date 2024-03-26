import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, map } from 'rxjs';
import { AbstractControl, AsyncValidatorFn, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { BaseFormComponent } from '../base-form.component';
import { CityService } from './city.service';
import type { City } from './city';
import type { Country } from './../countries/country';

@Component({
  selector: 'app-city-edit',
  templateUrl: './city-edit.component.html',
  styleUrls: ['./city-edit.component.scss']
})
export class CityEditComponent extends BaseFormComponent implements OnInit {
  title?: string;
  city?: City;
  // The city object id as fetched from the active route
  // NULL when adding a new city, and
  // NOT NULL when editing an existing city.
  id?: number;
  // The countries array for the select
  countries?: Array<Country>;

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private cityService: CityService) {
    super();
  }

  ngOnInit() {
    this.form = new FormGroup({
      name: new FormControl('', Validators.required),
      lat: new FormControl('', [
        Validators.required,
        Validators.pattern(/^\-?\d+(\.\d{1,4})?$/)
      ]),
      lon: new FormControl('', [
        Validators.required,
        Validators.pattern(/^\-?\d+(\.\d{1,4})?$/)
      ]),
      countryId: new FormControl('', Validators.required)
    }, null, this.isDupeCity());

    this.loadData();
  }

  loadData() {
    this.loadCountries();
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // EDIT mode
      this.cityService.get(this.id)
        .subscribe({
          next: (result) => {
            this.city = result;
            this.title = "Edit - " + this.city.name;
            this.form.patchValue(this.city);
          },
          error: (error) => console.error(error)
        });
    } else {
      // CREATE mode
      this.title = "Create a new City";
    }
  }

  loadCountries() {
    this.cityService.getCountries(
      0,
      9999,
      "name",
      "asc",
      null,
      null).subscribe({
        next: (result) => {
          this.countries = result.data;
        },
        error: (error) => console.error(error)
      });
  }

  onSubmit() {
    var city = (this.id) ? this.city : <City>{};
    if (city) {
      city.name = this.form.controls['name'].value;
      city.lat = +this.form.controls['lat'].value;
      city.lon = +this.form.controls['lon'].value;
      city.countryId = +this.form.controls['countryId'].value;

      if (this.id) {
        // EDIT mode
        this.cityService.put(city)
          .subscribe({
            next: () => {
              console.log("City " + city!.id + " has been updated.");
              this.router.navigate(['/cities']);
            },
            error: (error) => console.error(error)
          });
      } else {
        // CREATE mode
        this.cityService.post(city)
          .subscribe({
            next: (result) => {
              console.log("City " + result.id + " has been created.");
              this.router.navigate(['/cities']);
            },
            error: (error) => console.error(error)
          });
      }
    }
  }

  isDupeCity(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<{ [key: string]: any } | null> => {
      var city = <City>{};
      city.id = (this.id) ? this.id : 0;
      city.name = this.form.controls['name'].value;
      city.lat = +this.form.controls['lat'].value;
      city.lon = +this.form.controls['lon'].value;
      city.countryId = +this.form.controls['countryId'].value;

      return this.cityService.isDupeCity(city)
        .pipe(map(result => (result ? { isDupeCity: true } : null)));
    }
  }
}
