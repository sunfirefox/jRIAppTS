jRIAppTS With Generics
======

<b>RIA application framework for building LOB applications</b>
<br/>
This is a typescript version of the previous <a href="https://github.com/BBGONE/jRIApp" target="_blank">jRIApp framework</a>
You can watch video of the demo on <a href="http://youtu.be/m2lxFWhJghA" target="_blank">YouTube SPA</a>
This branch uses generics introduced in typescript. The primary goal of generics here is to reduce type castings and enhance productivity
with better intellisence and design time type checkings. 
The data service now can generate typescript classes from C# classes. This includes typed Dictionaries and Lists. See the DEMO project how it is implemented.
(DEMODB.ts uses autogenerated code.)
<br/>


<b>Future enhancements:</b>
<br/>
When the typescript will allow to define get and set properties in interfaces, there will be possible to see errors at design time when
one is trying to assign value to a read only property. Now it is only runtime errors those will notify about this.

<b>Differences from previous javascript version:</b>
<br/>
Now, strongly typed entities, DbSets, DbContext are pregenerated and we have typed classes at design time - they effectively include the Metadata in themselves.
To achieve this, the data service exposes <i>GetTypeScript</i> method.
You can enter in a browser an address to the GetTypeScript method on the ASP.NET MVC data controller, such as, for example
http://YOURSERVER/RIAppDemoService/GetTypeScript and you will get a strongly typed definition of the classes.
Then paste this text into some module (in the demo application it is in the demoDB.ts file).

Now, the Application class is not dependent on the db module and the DbContext's instance is created when you need it in your code.
You can remove the db module's reference (db.ts) from the jriapp.ts and the compiled jriapp.js will not be dependent on the data service.
You can replace the db module with a new implemetation, if you want to interact with differently implemented data service (for example, which uses OData format).
You can also remove references for listbox.ts, datadialog.ts, datagrid.ts, pager.ts, stackpanel.ts if you don't need their functionality in your
applications. But don't remove a reference for dataform.ts, because the Application class knows about it.

Previously, we used the Metadata to initialze a DbContex's instance, now, the typed DbContext needs only a part of that data,
so i added one more method on the data service which return only this information- it is named PermissionsInfo. This info
includes server's time zone and permissions for CRUD methods on the DbSets. They allow to enable - disable some controls on the client
depending on these permissions. But the permissions are always checked at the server too. So these permissions are only convenient way
to make UI more users friendly.

Because the core application class does not expose dbContext property, i added a way to register class instances in the application with
application's <i>registerObject</i> method. You can register DbContext's instance at application's startup, and later get this instance
with application's <i>getObject</i> method. I used this method to provide DbContext's instance to my custom autocomplete element view.
It gets DbContext's registered name in the options and obtains the DbContext's instance using getObject method.

Also changed the way to define calculated fields implementation. Previously Application and DbContext had 'define_calc' event to get them.
Now it is done in strongly typed way. We can use generatated on the DbSets methods to define each calculated field individually, in the
form <i>defineFIELDNameField</i>. You can see the demo for example.

The Data bindings had been enhanced. Now if you specify only the data-view attribute on the html element (without the data-bind attribute),
the element view on that DOM HTML element will still be created. It can be used in rare cases when you want to attach some custom code
to the DOM element, and you dont need to data bind to the element view's properties (for example to attach animation behavior).
 
One more change had been made to make data content attributes more typescript friendly.
Previously to define lookup content you did something like this: 
data-content="fieldName=ProductCategoryID,lookup:{dataSource=dbContext.dbSets.ProductCategory,valuePath=ProductCategoryID,textPath=Name}" 
now it is changed to:
data-content="fieldName=ProductCategoryID,name:lookup,options:{dataSource=dbContext.dbSets.ProductCategory,valuePath=ProductCategoryID,textPath=Name}"

the same applies for the multyline data content. Now the name and the options for specialized data contents are specified separately.

The rest of the framework's behavior and implementation is still the same as in previous javascript version.
You can use the docs from there to understand how the framework works and how to use it.

After minification jriapp.js has size about 290 KB. But it can be further gzipped, for it to reach the size of about 65 KB.
In my real world applications i use ASP.NET MVC 4 bundling feature. For desktop applications it will suffice.

You are welcome to use it in your applications.

<b>Latest changes:</b>

<p>2013-07-30   Bug fix in a dataform usage inside templates. Corrected data bindings in these cases.</p>
<p>2013-07-30   The DataService class enhancements.Now besides getting typescript like, for example, http://YOURSERVER/RIAppDemoService/GetTypeScript<br/>
The DataService now exposes two other methods to get XAML version of the metadata and C# implementation of the dataservice's methods.<br/>
 You can test them in the DEMO using<br/>
 http://YOURSERVER/RIAppDemoService/GetXAML<br/>
 http://YOURSERVER/RIAppDemoService/GetCSharp
</p>
<p>2013-08-31  Published generics version of the framework - <b>tested to work and to be compilable with 0.9.1.1 version of the compiler.</b></p>
<p>2013-09-07  Improved typed List and Dictionary code generation, to include properties' data types.</p>
<p>2013-09-09  List and Dictionary code modifications</p>
<p>2013-09-20  CSharp enums to typescript enums dataservice's code generation</p>
--
Maxim V. Tsapov<br/>
Moscow, Russian Federation<br/> 
<a href="https://plus.google.com/u/0/102838307743207067758/about?tab=wX" target="_blank">I'm on Google+</a>