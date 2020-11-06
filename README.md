# Node Editor

Библиотека для создания кастомных нодовых редакторов.

## Основные фичи
* Собственные ассеты для графов
* Кастомизация UI редактора
* Выполнение графов: синхронное и асинхронное
* Работает в Runtime и в Editor

## Установка
Для установки в проект необходимо поместить содержимое репозитория в любую папку внутри Assets. Требуется версия Unity не ниже 2019.1

# Использование

## Создание нового ассета графа

В самом минимальном виде вам нужно сделать следующее:
### 1. Создать новый тип графа

Для этого вам нужно создать класс, реализующий интерфейс `IGraphType`

```c#
public class MinimalGraphType : IGraphType
{
    private BaseConnectionType _defaultConnectionType = new BaseConnectionType();

    // название графа, которое будет отображаться в окне редактора
    public string Name => "Minimal Graph";
    // список всех типов узлов, которые могут содержаться в графе.
    // здесь мы создаем список сами, но можно использовать для этого отдельный реестр INodeTypeRegistry. об этом подробнее дальше.
    public IReadOnlyList<INodeType> AvailableNodes { get; } = new List<INodeType>() { new MinimalNodeType() };
    // возвращает тип соединения, которое может соединить выходной порт с типом from и входной порт с типом to
    // если указанные типы соединить нельзя, то метод должен вернуть null
    public IConnectionType FindConnectionType( Type from, Type to )
    {
        return _defaultConnectionType;
    }
    // вызывается после того, как граф был полностью загружен и инициализирован
    public void PostLoad( GraphModel graph )
    {
    }
}
```

### 2. Создать Importer для вашего ассета графа

Для этого вам понадобится класс, унаследованный от `BaseGraphAssetImporter`

```c#
// обязательный атрибут. он задает какому типу ассетов соответствует этот класс
[ScriptedImporter(0, ConstExtension, -1)]
public class MinimalGraphAssetImporter : BaseGraphAssetImporter
{
    // расширение файла, в котором будут хранится ассеты этого типа. должно быть уникальным, т.к. по нему Unity определяет, каким образом импортировать ассет
    public const string ConstExtension = "mingraph";
    // это свойство нужно для работы базового класса и должно возвращать то же расширение файла
    public override string Extension => ConstExtension;
    // задает тип окна редактора. если вам понадобится расширять возможности окна редактирования - укажите здесь тип своего окна
    public override Type EditorWindowType => typeof( BaseGraphEditorWindow );

    // пункт меню для создания нового ассета в проекте. этот метод не обязателен, вы можете создавать ассеты любым другим способом, но таким способом проще
    [MenuItem( "Assets/Create/Minimal Graph", false, 208 )]
    public static void CreateGraph()
    {
        var type = new MinimalGraphType();
        var graph = new GraphModel( type );

        CreateNewGraphAsset( graph, $"New Build Graph.{ConstExtension}" );
    }
}
```

### 3. Создать хотя бы один тип узлов

Для этого нужно создать класс, реализующий интерфейс `INodeType`. Но намного проще - унаследоваться от `BaseNodeType`:

```c#
public class MinimalNodeType : BaseNodeType
{
    // объявляем входной порт типа object и id = "In"
    public InputPort<object> In { get; }
    // объявляем выходной порт типа object и id = "Out"
    public OutputPort<object> Out { get; }
}
```

Все. Этого кода достаточно, чтобы можно было создать Asset в окне Project и открыть окно редактирования с помощью двойного клика по файлу.

### Обнаружение типов узлов через BaseNodeTypeRegistry

Вы можете автоматически обнаруживать доступные типы узлов через рефлексию с помощью класса `BaseNodeTypeRegistry`.

Для этого вам нужно унаследоваться от него и переопределить несколько методов:

```c#
public class MinimalNodeTypeRegistry : BaseNodeTypeRegistry
{
    public BuildNodeTypeRegistry()
    {
        // Опционально отключает поиск в Editor-only сборках
        ExcludeEditorAssemblies = true;
    }
    
    // Опционально фильтрует сборки, в которых следует искать типы
    protected override bool ShouldProcessAssembly( Assembly asm )
    {
        return asm.FullName.Contains( "MyProjectNodes" );
    }

    // Опционально фильтрует типы, которые могут быть использованы
    protected override bool ShouldProcessType( Type type )
    {
        return typeof( MinimalNodeType ).IsAssignableFrom( type );
    }
}
```

Затем используйте этот класс в вашем `GraphType`:

```c#
public class MinimalGraphType : IGraphType
{
    private INodeTypeRegistry _nodeTypeRegistry = new MinimalNodeTypeRegistry();

    public IReadOnlyList<INodeType> AvailableNodes => _nodeTypeRegistry.AvailableNodes;

    // остальной код ....
}
```

## Использование графа

Редактировать граф хорошо, а использовать полученный граф для каких-то практических целей - еще лучше. Для этого существуют методы `GraphModel.Execute` и `GraphModel.ExecuteAsync`. Но для того, чтобы их вызвать, нам надо добавить еще немного кода.

Добавим метод Execute в наш тип узла (`MinimalNodeType` из предыдущего шага):

```c#
    public void Execute( INodeExecutionContext ctx )
    {
        // получаем значение из входного порта
        var inputValue = In.Get( ctx );
        // рассчитываем выходное значение
        var outputValue = inputValue.ToString();
        // выставляем значение в выходной порт
        Out.Set( ctx, outputValue );
    }
```

Создадим собственный тип редактора графа. Он нужен, чтобы добавить дополнительные кнопки в тулбар.

```c#
public class MinimalGraphEditorView : BaseGraphEditorView
{
    public MinimalGraphEditorView()
    {
        // добавляем в тулбар окна кнопку, которая будет вызывать Execute нашего графа
        Toolbar.Add( new ToolbarButton( ExecuteGraph ) { text = "Execute Graph" } );
    }

    private void ExecuteGraph()
    {
        // вызываем выполнение текущего графа
        Graph.Execute( ExecuteNode );
    }

    // этот метод будет вызываться для каждого узла графа
    private void ExecuteNode( INodeExecutionContext ctx)
    {
        // получаем тип нашейго узла
        var nodeType = ctx.Node.Type as MinimalNodeType;
        // вызываем Execute конкретного типа узла
        nodeType.Execute( ctx );
    }
}
```

Создадим собственный тип окна редактора:

```c#
public class MinimalGraphEditorWindow : BaseGraphEditorWindow
{
    protected override BaseGraphEditorView CreateEditorView()
    {
        // создаем наш собственный редактор, вместо дефолтного
        return new MinimalGraphEditorView();
    }
}
```

Укажем наш тип окна в Importer'е ассета:

```c#
public class MinimalGraphAssetImporter : BaseGraphAssetImporter
{
    // ...
    public override Type EditorWindowType => typeof( MinimalGraphEditorWindow );
    // ...
}
```

Теперь мы можем выполнить наш граф, нажав на кнопку **Execute Graph** в окне редактора. Но мы получим ошибку, т.к. все наши ноды имеют входы. Чтобы ошибок не было, нужно реализовать ноду, у которой нет входов и использовать ее в самом начале графа.

### Правила выполнения графа

Выполнение графа - это операция обхода узлов и соединений графа в определенном порядке.

Сначала выполняются узлы, у которых нет входов. Эти узлы вычисляют выходные значения, которые передаются через соединения в следующие узлы. Для соединений тоже можно задать метод, который будет вызван при прохождении через каждое из них.

Дальше находится узел, у которого вычислены все входные значения и выполняется он. Этот шаг повторяется пока мы не обойдем все узлы, соединенные с начальными.

Если в графе останутся узлы, у которых не вычислены все входные значения, будет выведено сообщение в лог.

При асинхронном выполнении происходят те же действия. Отличие лишь в том, что для каждого элемента (узла или соединения) вызывается функция, возвращающая `Task`, и сам метод `ExecuteAsync` также возвращает `Task`.

## Кастомизация внешнего вида редактора

Каждый узел, порт и соединение рисуются с помощью отдельной View. Вы можете использовать собственные классы для отображения этих элементов.

Для этого вам надо переопределить соответствующие методы в классе редактора графа:

```c#
public override BaseNodeView CreateNodeView( NodeModel node, IEdgeConnectorListener edgeConnectorListener )
{
    return new MinimalGraphNodeView( node, edgeConnectorListener );
}

public override BaseConnectionView CreateConnectionView( ConnectionModel connection )
{
    return new MinimalGraphConnectionView( connection );
}
```

И во View узла:

```c#
protected virtual BasePortView CreatePortView( PortModel port )
{
    return new MinimalPortView( port );
}
```

Созданные вами View должны наследоваться от `BaseNodeView`, `BaseConnectionView` и `BasePortView` соответственно.

## Задание параметров узлов

У каждого типа узлов могут быть свои собственные параметры, которые доступны для редактирования в окне графа. По умолчанию никаких параметров не создается, но это несложно исправить.

Необходимо переопределить метод `CreatePropertyBlock` в классе `BaseNodeType`. Этот метод должен вернуть объект класса наследника `PropertyBlock`. Предпочтительным способом является создание вложенного класса внутри вашего `NodeType`:

```c#
public class MinimalNodeType : BaseNodeType
{
    protected override PropertyBlock CreatePropertyBlock( NodeModel node )
    {
        return new Properties();
    }

    public class Properties
    {
        public string MyProperty;
    }
}
```

Теперь в каждом узле этого типа будет отображаться редактор свойств. По умолчанию он поддерживает большинство простых типов, типы Unity (`Vector3`, `Color` и т.п.), списки, и вложенные объекты.

Но вы не ограничены редактором по умолчанию. Вы можете написать полностью свою View, которая будет отображаться в редакторе.

Чтобы ее подключить необходимо переопределить метод `CreatePropertiesView` в классе `BaseNodeView`:

```c#
public override VisualElement CreatePropertiesView()
{
    return new MyCustomView( Node.PropertyBlock );
}
```
