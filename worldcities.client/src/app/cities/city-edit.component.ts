import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl, FormGroup } from '@angular/forms';
import { environment } from '../../environments/environment';
import { City } from './city';
import { Country } from './../countries/country';

@Component({
  selector: 'app-city-edit',
  templateUrl: './city-edit.component.html',
  styleUrls: ['./city-edit.component.scss']
})
export class CityEditComponent implements OnInit {
  title?: string;
  form!: FormGroup;
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
    private http: HttpClient
  ) { }

  ngOnInit() {
    this.form = new FormGroup({
      name: new FormControl(''),
      lat: new FormControl(''),
      lon: new FormControl(''),
      countryId: new FormControl('')
    });

    this.loadData();
  }

  loadData() {
    this.loadCountries();
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // EDIT mode
      var url = environment.baseUrl + 'api/Cities/' + this.id;

      this.http.get<City>(url).subscribe({
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
    var url = environment.baseUrl + 'api/Countries';
    var params = new HttpParams()
      .set("pageIndex", "0")
      .set("pageSize", "9999")
      .set("sortColumn", "name");
    this.http.get<any>(url, { params }).subscribe({
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
        var url = environment.baseUrl + 'api/Cities/' + city.id;

        this.http.put<City>(url, city).subscribe({
          next: () => {
            console.log("City " + city!.id + " has been updated.");
            this.router.navigate(['/cities']);
          },
          error: (error) => console.error(error)
        });
      } else {
        // CREATE mode
        var url = environment.baseUrl + 'api/Cities';
        this.http.post<City>(url, city).subscribe({
          next: (result) => {
            console.log("City " + result.id + " has been created.");
            this.router.navigate(['/cities']);
          },
          error: (error) => console.error(error)
        });
      }
    }
  }
}
